using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace Scellecs.Morpeh.Transforms
{
#if MORPEH_ELYSIUM
    public sealed class LocalToWorldSystem : Elysium.IUpdateSystem
#else
    public sealed class LocalToWorldSystem : ISystem
#endif
    {
        public World World { get; set; }

        private Filter rootFilter;
        private Filter parentFilter;

        private Stash<LocalToWorld> localToWorldStash;
        private Stash<LocalTransform> localTransformStash;
        private Stash<PostTransformMatrix> postTransformMatrixStash;
        private Stash<Parent> parentStash;
        private Stash<Child> childStash;

        public void OnAwake()
        {
            rootFilter = World.Filter
                .With<LocalToWorld>()
                .With<LocalTransform>()
                .Without<Parent>()
                .Build();

            parentFilter = World.Filter
                .With<LocalTransform>()
                .With<Child>()
                .Without<Parent>()
                .Build();

            localToWorldStash = World.GetStash<LocalToWorld>();
            localTransformStash = World.GetStash<LocalTransform>();
            postTransformMatrixStash = World.GetStash<PostTransformMatrix>();
            parentStash = World.GetStash<Parent>();
            childStash = World.GetStash<Child>();
        }

        public void OnUpdate(float deltaTime)
        {
            var rootFilterNative = rootFilter.AsNative();
            var parentFilterNative = parentFilter.AsNative();

            var localToWorldStashNative = localToWorldStash.AsNative();
            var localTransformStashNative = localTransformStash.AsNative();
            var postTransformMatrixStashNative = postTransformMatrixStash.AsNative();
            var parentStashNative = parentStash.AsNative();
            var childStashNative = childStash.AsNative();

            var rootJob = new ComputeRootLocalToWorldJob
            {
                rootFilter = rootFilterNative,
                localToWorldStash = localToWorldStashNative,
                localTransformStash = localTransformStashNative,
                postTransformMatrixStash = postTransformMatrixStashNative
            }
            .ScheduleParallel(rootFilterNative.length, 32, default);

            var childJob = new ComputeChildLocalToWorldJob
            {
                parentFilter = parentFilterNative,
                localToWorldStash = localToWorldStashNative,
                localTransformStash = localTransformStashNative,
                postTransformMatrixStash = postTransformMatrixStashNative,
                parentStash = parentStashNative,
                childStash = childStashNative
            }
            .ScheduleParallel(parentFilterNative.length, 32, rootJob);

            childJob.Complete();
        }

        public void Dispose() { }
    }

    [BurstCompile]
    internal struct ComputeRootLocalToWorldJob : IJobFor
    {
        public NativeFilter rootFilter;
        public NativeStash<LocalToWorld> localToWorldStash;
        public NativeStash<LocalTransform> localTransformStash;
        public NativeStash<PostTransformMatrix> postTransformMatrixStash;

        public void Execute(int index)
        {
            var entityId = rootFilter[index];
            var localTransform = localTransformStash.Get(entityId);
            var postTransformMatrix = postTransformMatrixStash.Get(entityId, out bool hasPostTransformMatrix);
            ref var localToWorld = ref localToWorldStash.Get(entityId);

            localToWorld.value = hasPostTransformMatrix ? math.mul(localTransform.ToMatrix(), postTransformMatrix.Value) : localTransform.ToMatrix();
        }
    }

    [BurstCompile]
    internal struct ComputeChildLocalToWorldJob : IJobFor
    {
        public NativeFilter parentFilter;
        public NativeStash<LocalToWorld> localToWorldStash;
        public NativeStash<LocalTransform> localTransformStash;
        public NativeStash<PostTransformMatrix> postTransformMatrixStash;
        public NativeStash<Parent> parentStash;
        public NativeStash<Child> childStash;

        public void Execute(int index)
        {
            var entityId = parentFilter[index];
            var children = childStash.Get(entityId);
            var localToWorld = localToWorldStash.Get(entityId);

            for (int i = 0; i < children.Value.Length; i++)
            {
                ChildLocalToWorldFromTransformMatrix(localToWorld.value, children.Value[i]);
            }
        }

        private void ChildLocalToWorldFromTransformMatrix(in float4x4 parentLocalToWorld, Entity childEntity)
        {
            ref var localToWorld = ref localToWorldStash.Get(childEntity, out bool hasLocalToWorld);
            var localTransform = localTransformStash.Get(childEntity, out bool hasLocalTransform);
            var hasParent = parentStash.Has(childEntity);
            var matchesLocalToWorldWriteGroupMask = hasLocalToWorld & hasLocalTransform & hasParent;

            if (matchesLocalToWorldWriteGroupMask)
            {
                var ltw = math.mul(parentLocalToWorld, localTransform.ToMatrix());
                var postTransformMatrix = postTransformMatrixStash.Get(childEntity, out bool hasPostTransformMatrix);

                if (hasPostTransformMatrix)
                {
                    ltw = math.mul(ltw, postTransformMatrix.Value);
                }

                localToWorld = new LocalToWorld() { value = ltw };
            }

            var children = childStash.Get(childEntity, out bool hasChildren);

            if (hasChildren)
            {
                for (int i = 0; i < children.Value.Length; i++)
                {
                    ChildLocalToWorldFromTransformMatrix(localToWorld.value, children.Value[i]);
                }
            }
        }
    }
}
