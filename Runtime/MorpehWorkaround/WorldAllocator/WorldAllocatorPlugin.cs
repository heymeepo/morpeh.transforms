using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Scellecs.Morpeh.Workaround.WorldAllocator
{
    [Preserve]
    internal class WorldAllocatorPlugin : IWorldPlugin
    {
        internal static Dictionary<int, WorldRewindableAllocator> rwdAllocators;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void RuntimeInitialize()
        {
            WorldExtensions.AddWorldPlugin(new WorldAllocatorPlugin());
            rwdAllocators = new Dictionary<int, WorldRewindableAllocator>();
        }

        public void Initialize(World world)
        {
            rwdAllocators.Add(world.identifier, WorldRewindableAllocator.Create(world));

            var sysGroup = world.CreateSystemsGroup();
            sysGroup.AddSystem(new WorldAllocatorRewindSystem());
            world.AddPluginSystemsGroup(sysGroup);
        }

        public void Deinitialize(World world)
        {
            if (rwdAllocators.TryGetValue(world.identifier, out var allocator))
            {
                allocator.rwdAllocator.Allocator.Dispose();
                allocator.rwdAllocator.Dispose();
                allocator.world = null;
                rwdAllocators.Remove(world.identifier);
            }
        }
    }
}
