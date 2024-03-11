#if MORPEH_BURST
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

namespace Scellecs.Morpeh.Workaround.WorldAllocator
{
    public unsafe struct WorldUpdateAllocator
    {
        internal DoubleRewindableAllocators* rwdAllocator;
        internal World world;

        public WorldUpdateAllocator(World world)
        {
            const int INITIAL_BLOCK_SIZE = 128 * 1024; //128kb

            rwdAllocator = (DoubleRewindableAllocators*)UnsafeUtility.Malloc(sizeof(DoubleRewindableAllocators), JobsUtility.CacheLineSize, Allocator.Persistent);
            rwdAllocator[0] = new DoubleRewindableAllocators(Allocator.Persistent, INITIAL_BLOCK_SIZE);

            this.world = world;
        }

        internal void Dispose()
        {
            rwdAllocator->Dispose();
            UnsafeUtility.Free(rwdAllocator, Allocator.Persistent);
        }
    }
}
#endif
