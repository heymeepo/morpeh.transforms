using Scellecs.Morpeh.Native;
using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
#if MORPEH_ELYSIUM
using Scellecs.Morpeh.Elysium;
#endif

namespace Scellecs.Morpeh.Transforms
{
#if MORPEH_ELYSIUM
    public sealed class ParentSystem : IUpdateSystem
#else
    public sealed class ParentSystem : ISystem
#endif
    {
        public World World { get; set; }

        private Filter deletedParentsFilter;
        private Filter newParentsFilter;
        private Filter removedParentsFilter;
        private Filter existingParentsFilter;

        private Stash<Child> childStash;
        private Stash<Parent> parentStash;
        private Stash<PreviousParent> previousParentStash;
        private Stash<ParentChangedMarker> parentChangedStash;

        public void OnAwake()
        {
            deletedParentsFilter = World.Filter
                .With<Child>()
                .Without<LocalToWorld>()
                .Build();

            newParentsFilter = World.Filter
                .With<Parent>()
                .Without<PreviousParent>()
                .Build();

            removedParentsFilter = World.Filter
                .With<PreviousParent>()
                .Without<Parent>()
                .Build();

            existingParentsFilter = World.Filter
                .With<Parent>()
                .With<PreviousParent>()
                .With<ParentChangedMarker>()
                .Build();

            childStash = World.GetStash<Child>().AsDisposable();
            parentStash = World.GetStash<Parent>();
            previousParentStash = World.GetStash<PreviousParent>();
            parentChangedStash = World.GetStash<ParentChangedMarker>();
        }

        public void OnUpdate(float deltaTime)
        {
            UpdateDeletedParents();
            UpdateRemoveParents();
            UpdateNewParents();
            UpdateChangeParents();
            CleanupChangedParentMarkers();
        }

        private void UpdateDeletedParents()
        {
            if (deletedParentsFilter.IsEmpty())
            {
                return;
            }

            var childEntities = new NativeQueue<EntityId>(Allocator.TempJob);
            var parentsFilter = deletedParentsFilter.AsNative();

            new GatherChildEntitiesJob()
            {
                parentsFilter = parentsFilter,
                childStash = childStash.AsNative(),
                parentsStash = parentStash.AsNative(),
                children = childEntities.AsParallelWriter()
            }
            .ScheduleParallel(parentsFilter.length, 32, default).Complete();

            while (childEntities.TryDequeue(out var childId))
            {
                if (World.TryGetEntity(childId, out var childEnt))
                {
                    parentStash.Remove(childEnt);
                    previousParentStash.Remove(childEnt);
                }
            }

            childEntities.Dispose(default).Complete();

            foreach (var entity in deletedParentsFilter)
            {
                childStash.Remove(entity);
            }

            World.Commit();
        }

        private void UpdateRemoveParents()
        {
            foreach (var childEntity in removedParentsFilter)
            {
                ref var prevParent = ref previousParentStash.Get(childEntity);

                if (World.TryGetEntity(prevParent.Value, out var parentEntity))
                {
                    RemoveChildFromParent(childEntity, parentEntity);
                }

                previousParentStash.Remove(childEntity);
            }

            World.Commit();

            void RemoveChildFromParent(Entity childEntity, Entity parentEntity)
            {
                ref var children = ref childStash.Get(parentEntity, out bool hasChildren).Value;

                if (hasChildren)
                {
                    var childIndex = FindChildIndex(children, childEntity);
                    children.RemoveAt(childIndex);

                    if (children.Length == 0)
                    {
                        childStash.Remove(parentEntity);
                    }
                }
            }

            int FindChildIndex(NativeList<EntityId> children, Entity entity)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i] == entity.ID)
                        return i;
                }

                throw new InvalidOperationException("Child entity not in parent");
            }
        }

        private void UpdateNewParents()
        {
            foreach (var entity in newParentsFilter)
            {
                previousParentStash.Set(entity, new PreviousParent() { Value = EntityId.Invalid });
            }

            World.Commit();
        }

        private unsafe void UpdateChangeParents()
        {
            if (existingParentsFilter.IsEmpty())
            {
                return;
            }

            var parentsFilterNative = existingParentsFilter.AsNative();
            var count = parentsFilterNative.length * 2;

            var parentChildrenToRemove = new NativeParallelMultiHashMap<EntityId, EntityId>(count, Allocator.TempJob);
            var parentChildrenToAdd = new NativeParallelMultiHashMap<EntityId, EntityId>(count, Allocator.TempJob);
            var uniqueParents = new NativeParallelHashMap<EntityId, int>(count, Allocator.TempJob);
            var childParentToRemove = new NativeParallelHashSet<EntityId>(count, Allocator.TempJob);

            var gatherChangedParentsJobHandle = new GatherChangedParentsJob
            {
                parentChildrenToAdd = parentChildrenToAdd.AsParallelWriter(),
                parentChildrenToRemove = parentChildrenToRemove.AsParallelWriter(),
                childParentToRemove = childParentToRemove.AsParallelWriter(),
                uniqueParents = uniqueParents.AsParallelWriter(),
                world = World.AsNative(),
                existingParentsFilter = existingParentsFilter.AsNative(),
                parentStash = parentStash.AsNative(),
                previousParentStash = previousParentStash.AsNative(),
                childStash = childStash.AsNative(),
            }
            .ScheduleParallel(parentsFilterNative.length, 16, default);
            gatherChangedParentsJobHandle.Complete();

            foreach (var entityId in childParentToRemove)
            {
                if (World.TryGetEntity(entityId, out var entity))
                {
                    parentStash.Remove(entity);
                }
            }

            World.Commit();

            var parentsMissingChild = new NativeList<EntityId>(Allocator.TempJob);

            var parentsMissingChildHandle = new FindMissingChildJob()
            {
                childStash = childStash.AsNative(),
                uniqueParents = uniqueParents,
                parentsMissingChild = parentsMissingChild
            }
            .Schedule();
            parentsMissingChildHandle.Complete();

            for (int i = 0; i < parentsMissingChild.Length; i++)
            {
                if (World.TryGetEntity(parentsMissingChild[i], out var missingChildEntity))
                {
                    childStash.Set(missingChildEntity, new Child() { Value = new NativeList<EntityId>(Allocator.Persistent) });
                }
            }

            World.Commit();

            new FixupChangedChildrenJob()
            {
                childStash = childStash.AsNative(),
                parentChildrenToAdd = parentChildrenToAdd,
                parentChildrenToRemove = parentChildrenToRemove,
                uniqueParents = uniqueParents
            }
            .Schedule().Complete();

            var parents = uniqueParents.GetKeyArray(Allocator.Temp);

            foreach (var parentEntityId in parents)
            {
                if (World.TryGetEntity(parentEntityId, out var parentEntity))
                {
                    var children = childStash.Get(parentEntity);

                    if (children.Value.Length == 0)
                    {
                        childStash.Remove(parentEntity);
                    }
                }
            }

            JobHandle* disposeHandles = stackalloc JobHandle[5];
            disposeHandles[0] = parentChildrenToRemove.Dispose(default);
            disposeHandles[1] = parentChildrenToAdd.Dispose(default);
            disposeHandles[2] = uniqueParents.Dispose(default);
            disposeHandles[3] = childParentToRemove.Dispose(default);
            disposeHandles[4] = parentsMissingChild.Dispose(default);

            JobHandleUnsafeUtility.CombineDependencies(disposeHandles, 5).Complete();
        }

        private void CleanupChangedParentMarkers() => parentChangedStash.RemoveAll();

        public void Dispose() { }
    }

    [BurstCompile]
    internal struct GatherChildEntitiesJob : IJobFor
    {
        [ReadOnly] public NativeFilter parentsFilter;
        [ReadOnly] public NativeStash<Child> childStash;
        [ReadOnly] public NativeStash<Parent> parentsStash;
        [WriteOnly] public NativeQueue<EntityId>.ParallelWriter children;

        public void Execute(int index)
        {
            var parentEntityId = parentsFilter[index];
            var child = childStash.Get(parentEntityId);

            for (int i = 0; i < child.Value.Length; i++)
            {
                var childEntityId = child.Value[i];
                var parentFromChild = parentsStash.Get(childEntityId, out bool hasParent);

                if (hasParent && parentFromChild.Value == parentEntityId)
                {
                    children.Enqueue(childEntityId);
                }
            }
        }
    }

    [BurstCompile]
    internal struct FixupChangedChildrenJob : IJob
    {
        public NativeStash<Child> childStash;

        [ReadOnly] public NativeParallelMultiHashMap<EntityId, EntityId> parentChildrenToAdd;
        [ReadOnly] public NativeParallelMultiHashMap<EntityId, EntityId> parentChildrenToRemove;
        [ReadOnly] public NativeParallelHashMap<EntityId, int> uniqueParents;

        public void Execute()
        {
            var parents = uniqueParents.GetKeyArray(Allocator.Temp);

            for (int i = 0; i < parents.Length; i++)
            {
                var parent = parents[i];
                ref var children = ref childStash.Get(parent, out bool hasChildren);

                if (hasChildren)
                {
                    RemoveChildrenFromParent(parent, ref children.Value);
                    AddChildrenToParent(parent, ref children.Value);
                }
            }
        }

        void AddChildrenToParent(EntityId parent, ref NativeList<EntityId> children)
        {
            if (parentChildrenToAdd.TryGetFirstValue(parent, out var child, out var it))
            {
                do
                {
                    children.Add(child);
                }
                while (parentChildrenToAdd.TryGetNextValue(out child, ref it));
            }
        }

        void RemoveChildrenFromParent(EntityId parent, ref NativeList<EntityId> children)
        {
            if (parentChildrenToRemove.TryGetFirstValue(parent, out var child, out var it))
            {
                do
                {
                    var childIndex = FindChildIndex(ref children, child);
                    children.RemoveAt(childIndex);
                }
                while (parentChildrenToRemove.TryGetNextValue(out child, ref it));
            }
        }

        int FindChildIndex(ref NativeList<EntityId> children, EntityId entity)
        {
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] == entity)
                    return i;
            }

            ThrowChildEntityNotInParent();
            return -1;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void ThrowChildEntityNotInParent()
        {
            throw new InvalidOperationException("Child entity not in parent");
        }
    }

    [BurstCompile]
    internal struct FindMissingChildJob : IJob
    {
        public NativeStash<Child> childStash;
        public NativeParallelHashMap<EntityId, int> uniqueParents;
        public NativeList<EntityId> parentsMissingChild;

        public void Execute()
        {
            var parents = uniqueParents.GetKeyArray(Allocator.Temp);

            for (int i = 0; i < parents.Length; i++)
            {
                var parent = parents[i];

                if (childStash.Has(parent) == false)
                {
                    parentsMissingChild.Add(parent);
                }
            }
        }
    }

    [BurstCompile]
    internal struct GatherChangedParentsJob : IJobFor
    {
        public NativeWorld world;
        public NativeFilter existingParentsFilter;
        public NativeStash<Parent> parentStash;
        public NativeStash<PreviousParent> previousParentStash;
        public NativeStash<Child> childStash;

        public NativeParallelMultiHashMap<EntityId, EntityId>.ParallelWriter parentChildrenToAdd;
        public NativeParallelMultiHashMap<EntityId, EntityId>.ParallelWriter parentChildrenToRemove;
        public NativeParallelHashSet<EntityId>.ParallelWriter childParentToRemove;
        public NativeParallelHashMap<EntityId, int>.ParallelWriter uniqueParents;

        public void Execute(int index)
        {
            var childEntityId = existingParentsFilter[index];

            ref var parent = ref parentStash.Get(childEntityId);
            ref var previousParent = ref previousParentStash.Get(childEntityId);

            if (parent.Value != previousParent.Value)
            {
                if (world.Has(parent.Value) == false)
                {
                    childParentToRemove.Add(childEntityId);
                    return;
                }

                parentChildrenToAdd.Add(parent.Value, childEntityId);
                uniqueParents.TryAdd(parent.Value, 0);

                if (childStash.Has(previousParent.Value))
                {
                    parentChildrenToRemove.Add(previousParent.Value, childEntityId);
                    uniqueParents.TryAdd(previousParent.Value, 0);
                }

                previousParent.Value = parent.Value;
            }
        }
    }
}
