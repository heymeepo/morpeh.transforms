using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Prototypes.Core.ECS.MorpehWorkaround
{
    internal static class CleanupComponentsHelper
    {
        private static HashSet<Type> cleanupTypes;

        private static IntHashSet nonCleanupOffsets = new IntHashSet();
        private static IntHashSet cleanupOffsets = new IntHashSet();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsCleanupComponent(ref CommonTypeIdentifier.InternalTypeDefinition definition)
        {
            var offset = definition.offset;

            if (cleanupTypes == null || cleanupTypes.Count == 0)
            {
                Load();
            }

            if (nonCleanupOffsets.Has(offset))
            {
                return false;
            }

            if (cleanupOffsets.Has(offset))
            {
                return true;
            }

            if (cleanupTypes.Contains(definition.type))
            {
                cleanupOffsets.Add(offset);
                return true;
            }
            else
            {
                nonCleanupOffsets.Add(offset);
                return false;
            }
        }

        private static void Load()
        {
            var types = GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsValueType && typeof(ICleanupComponent).IsAssignableFrom(type));

            cleanupTypes = new HashSet<Type>(types);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Cleanup()
        {
            cleanupTypes?.Clear();
            cleanupOffsets?.Clear();
            nonCleanupOffsets?.Clear();
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(a =>
                !a.GlobalAssemblyCache &&
                !a.FullName.StartsWith("mscorlib") &&
                !a.FullName.StartsWith("netstandard") &&
                !a.FullName.StartsWith("nunit") &&
                !a.FullName.StartsWith("System") &&
                !a.FullName.StartsWith("UnityEngine") &&
                !a.FullName.StartsWith("UnityEditor") &&
                !a.FullName.StartsWith("Unity") &&
                !a.FullName.StartsWith("Mono") &&
                !a.FullName.StartsWith("Bee") &&
                !a.FullName.StartsWith("Newtonsoft"));
        }
    }
}
