#if MORPEH_BURST
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Scellecs.Morpeh.Workaround.WorldAllocator
{
    [Preserve]
    internal sealed class WorldAllocatorPlugin : IWorldPlugin
    {
        internal static Dictionary<int, WorldUpdateAllocator> rwdAllocators;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void RuntimeInitialize()
        {
            rwdAllocators = new Dictionary<int, WorldUpdateAllocator>();
            WorldExtensions.AddWorldPlugin(new WorldAllocatorPlugin());
        }

        public void Initialize(World world)
        {
            rwdAllocators.Add(world.identifier, new WorldUpdateAllocator(world));

            var sysGroup = world.CreateSystemsGroup();
            sysGroup.AddSystem(new WorldAllocatorRewindSystem());
            world.AddPluginSystemsGroup(sysGroup);
        }

        public void Deinitialize(World world)
        {
            if (rwdAllocators.TryGetValue(world.identifier, out var allocator))
            {
                allocator.Dispose();
                allocator.world = null;
                rwdAllocators.Remove(world.identifier);
            }
        }
    }
}
#endif