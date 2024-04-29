using Scellecs.Morpeh.Collections;
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh.Workaround
{
    public static class ArchetypeWorkaroundExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetArchetypeHash(this Archetype archetype) => archetype.hash.GetValue();

        /// <summary>
        /// Don't modify anything inside, use for readonly access
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntHashSet GetArchetypeComponents(Archetype archetype) => archetype.components;
    }
}
