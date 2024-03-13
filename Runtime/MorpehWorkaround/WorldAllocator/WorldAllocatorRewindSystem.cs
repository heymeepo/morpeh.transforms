#if MORPEH_BURST
using Unity.Collections;

namespace Scellecs.Morpeh.Workaround.WorldAllocator
{
    internal unsafe sealed class WorldAllocatorRewindSystem : ICleanupSystem
    {
        public World World { get; set; }

        private DoubleRewindableAllocators* worldAllocator;

        public void OnAwake() => worldAllocator = World.GetUpdateAllocator().rwdAllocator;

        public void OnUpdate(float deltaTime) => worldAllocator->Update();

        public void Dispose() { }
    }
}
#endif
