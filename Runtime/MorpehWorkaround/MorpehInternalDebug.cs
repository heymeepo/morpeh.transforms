using Scellecs.Morpeh;
using System;
using System.Linq;

namespace Prototypes.Core.ECS.MorpehWorkaround
{
    public static class MorpehInternalDebug
    {
        public static int GetSystemGroupOrder(ISystem system)
        {
            foreach (var pair in system.World.systemsGroups)
            {
                if (pair.Value.systems.data.Contains(system))
                {
                    return pair.Key;
                }
            }

            return -1;
        }
    }
}
