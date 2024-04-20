using Scellecs.Morpeh.Collections;
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh.Workaround
{
    public static class ArchetypeWorkaroundExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetArchetypeHash(this Archetype archetype) => archetype.hash.GetValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntHashSet GetArchetypeComponentsDangerous(Archetype archetype) => archetype.components;
    }
}
