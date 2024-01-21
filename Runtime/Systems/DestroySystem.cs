using Prototypes.Core.ECS.MorpehWorkaround;
using UnityEngine;

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
                MorpehInternalTools.RemoveAllExceptCleanupComponents(entity);
            }
        }

        public void Dispose() { }
    }
}
