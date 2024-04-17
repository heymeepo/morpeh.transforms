using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Scellecs.Morpeh.Workaround
{
    internal static class CleanupComponentsHelper
    {
        private const int CLEANUP_COMPONENT = 1;
        private const int NON_CLEANUP_COMPONENT = -1;
        private const int UNMAPPED = 0;

        private static HashSet<Type> cleanupTypes;
        private static int[] cleanupIds;

        static CleanupComponentsHelper() => Load();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsCleanupComponent(int id)
        {
            if (id >= cleanupIds.Length)
            {
                ResizeMap(id);
            }

            var value = cleanupIds[id];

            if (value == UNMAPPED)
            {
                return MapStash(id) == CLEANUP_COMPONENT;
            }

            return value == CLEANUP_COMPONENT;
        }

        internal static void Load()
        {
            if (cleanupTypes == null)
            {
                var types = ReflectionHelpers.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsValueType && typeof(ICleanupComponent).IsAssignableFrom(type));

                cleanupTypes = new HashSet<Type>(types);
                cleanupIds = new int[WorldConstants.DEFAULT_STASHES_CAPACITY];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int MapStash(int id)
        {
            var componentType = MorpehInternalTools.GetComponentType(id);
            int result = cleanupTypes.Contains(componentType) ? CLEANUP_COMPONENT : NON_CLEANUP_COMPONENT;
            cleanupIds[id] = result;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ResizeMap(int byId)
        {
            var logBase2 = Math.Log(byId, 2);
            int nextPower = (int)Math.Ceiling(logBase2);
            int result = 1 << nextPower;

            Array.Resize(ref cleanupIds, result);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Cleanup()
        {
            cleanupTypes = null;
            cleanupIds = null;
        }
    }
}
