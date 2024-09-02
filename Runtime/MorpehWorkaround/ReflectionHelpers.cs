using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;

namespace Scellecs.Morpeh.Workaround.Utility
{
    public static class ReflectionHelpers
    {
        public static IEnumerable<Assembly> GetAssemblies()
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

        public static IEnumerable<Type> GetTypesWithAttribute<T>(this IEnumerable<Assembly> assemblies) where T : Attribute
        {
            return assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttributes(typeof(T), true).Length > 0);
        }

        public static T GetAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).FirstOrDefault() as T;
        }

        public static int[] GetOffsets<T>(Type type)
        {
            var offsets = new List<int>();
            GetOffsets<T>(type, offsets, 0);
            return offsets.Count > 0 ? offsets.ToArray() : null;
        }

        private static void GetOffsets<T>(Type type, List<int> offsets, int baseOffset)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                var offset = UnsafeUtility.GetFieldOffset(field) + baseOffset;

                if (field.FieldType == typeof(T))
                {
                    offsets.Add(offset);
                }
                else if (field.FieldType.IsValueType && !field.FieldType.IsPrimitive && field.FieldType != typeof(T))
                {
                    GetOffsets<T>(field.FieldType, offsets, offset);
                }
            }
        }
    }
}

