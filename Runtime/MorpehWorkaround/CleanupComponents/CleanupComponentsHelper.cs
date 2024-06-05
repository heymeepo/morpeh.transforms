using Scellecs.Morpeh.Workaround.Utility;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsCleanupComponent(int typeId)
        {
            if (cleanupTypes == null)
            {
                Load();
            }

            if (typeId >= cleanupIds.Length)
            {
                ResizeMap(typeId);
            }

            var value = cleanupIds[typeId];

            if (value == UNMAPPED)
            {
                return MapStash(typeId) == CLEANUP_COMPONENT;
            }

            return value == CLEANUP_COMPONENT;
        }

        internal static void Load()
        {
            var types = ReflectionHelpers.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsValueType && typeof(ICleanupComponent).IsAssignableFrom(type));

            cleanupTypes = new HashSet<Type>(types);
            cleanupIds = new int[WorldConstants.DEFAULT_STASHES_CAPACITY];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int MapStash(int typeId)
        {
            var componentType = MorpehInternalTools.GetComponentType(typeId);
            int result = cleanupTypes.Contains(componentType) ? CLEANUP_COMPONENT : NON_CLEANUP_COMPONENT;
            cleanupIds[typeId] = result;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ResizeMap(int byId)
        {
            int newSize = cleanupIds.Length;

            while (newSize <= byId) 
            {
                newSize <<= 1;
            }

            Array.Resize(ref cleanupIds, newSize);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Cleanup()
        {
            cleanupTypes = null;
            cleanupIds = null;
        }
    }
}
