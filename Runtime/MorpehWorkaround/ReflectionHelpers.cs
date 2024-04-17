using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

namespace Scellecs.Morpeh.Workaround
{
    internal static class ReflectionHelpers
    {
        internal static IEnumerable<Assembly> GetAssemblies()
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

