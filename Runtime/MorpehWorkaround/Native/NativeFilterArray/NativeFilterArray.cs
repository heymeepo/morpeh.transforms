#if MORPEH_BURST
using Scellecs.Morpeh.Native;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Scellecs.Morpeh.Workaround
{
    public unsafe struct NativeFilterArray
    {
        public EntityId this[int index]
        {
            get
            {
                var entityId = entities[index];
                var gen = world.entitiesGens[entityId];
                return new EntityId(entityId, gen);
            }
        }

        [ReadOnly]
        public readonly JobHandle buildCompletionHandle;

        [ReadOnly]
        public readonly int length;

        [ReadOnly, NativeDisableUnsafePtrRestriction]
        public int* entities;

        [ReadOnly]
        public NativeWorld world;

        internal NativeFilterArray(JobHandle buildCompletionHandle, NativeWorld world, int length, int* entities)
        {
            this.buildCompletionHandle = buildCompletionHandle;
            this.length = length;
            this.world = world;
            this.entities = entities;
        }
    }
}
#endif
