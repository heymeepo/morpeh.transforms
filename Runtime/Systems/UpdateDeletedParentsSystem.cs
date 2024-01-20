using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Scellecs.Morpeh.Transforms
{
    public sealed class UpdateDeletedParentsSystem : ICleanupSystem
    {
        public World World { get; set; }

        private Filter deletedParentsFilter;

        private Stash<Child> childStash;
        private Stash<Parent> parentStash;
        private Stash<PreviousParent> previousParentStash;

        public void OnAwake()
        {
            deletedParentsFilter = World.Filter
                .With<Child>()
                .With<DestroyMarker>()
                .Build();

            childStash = World.GetStash<Child>().AsDisposable();
            parentStash = World.GetStash<Parent>();
            previousParentStash = World.GetStash<PreviousParent>();
        }

        public void OnUpdate(float deltaTime)
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
        }

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
}
