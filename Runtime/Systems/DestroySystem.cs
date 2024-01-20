namespace Scellecs.Morpeh.Transforms
{
    public sealed class DestroySystem : ICleanupSystem
    {
        public World World { get; set; }

        private Filter destroyFilter;

        public void OnAwake()
        {
            destroyFilter = World.Filter.With<DestroyMarker>().Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in destroyFilter)
            {
                entity.Dispose();
            }
        }

        public void Dispose() { }
    }
}
