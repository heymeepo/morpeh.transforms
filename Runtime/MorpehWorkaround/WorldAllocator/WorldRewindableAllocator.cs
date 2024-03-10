using Unity.Collections;

namespace Scellecs.Morpeh.Workaround.WorldAllocator
{
    public struct WorldRewindableAllocator
    {
        internal AllocatorHelper<RewindableAllocator> rwdAllocator;
        internal World world;

        internal static WorldRewindableAllocator Create(World world)
        {
            var rwd = new AllocatorHelper<RewindableAllocator>(Allocator.Persistent);
            rwd.Allocator.Initialize(128 * 1024, false);
            return new WorldRewindableAllocator() { rwdAllocator = rwd, world = world };
        }
    }
}
