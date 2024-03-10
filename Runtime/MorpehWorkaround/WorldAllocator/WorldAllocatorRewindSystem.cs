using System.Runtime.CompilerServices;
using Unity.Collections;

namespace Scellecs.Morpeh.Workaround.WorldAllocator
{
    internal unsafe sealed class WorldAllocatorRewindSystem : ICleanupSystem
    {
        public World World { get; set; }

        private AllocatorHelper<RewindableAllocator> worldAllocator;

        public void OnAwake() => worldAllocator = World.RewindableAllocator().rwdAllocator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnUpdate(float deltaTime) => worldAllocator.Allocator.Rewind();

        public void Dispose() { }
    }
}
