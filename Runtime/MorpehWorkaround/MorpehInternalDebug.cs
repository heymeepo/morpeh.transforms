using System;
using System.Linq;

namespace Scellecs.Morpeh.Workaround
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

        public static int ArchetypesCount(Filter filter) => filter.archetypes.length;
    }
}
