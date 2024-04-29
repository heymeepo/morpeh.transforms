using System;
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh.Workaround
{
    public static class MorpehInternalTools
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetComponentBoxed(this Entity entity, object component)
        {
            var type = component.GetType();
            var helper = InternalHelperTypeAssociation.Get(type);
            helper.SetComponentBoxed(entity, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetComponentBoxed(this Entity entity, object component, out int typeId)
        {
            var type = component.GetType();
            var helper = InternalHelperTypeAssociation.Get(type);
            helper.SetComponentBoxed(entity, component);
            typeId = helper.GetTypeInfo().id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponentBoxed(this Entity entity, object component)
        {
            var type = component.GetType();
            var helper = InternalHelperTypeAssociation.Get(type);
            helper.RemoveComponentBoxed(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponentByType(this Entity entity, Type componentType)
        {
            var helper = InternalHelperTypeAssociation.Get(componentType);
            helper.RemoveComponentBoxed(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponentByTypeId(this Entity entity, int typeId)
        {
            var helper = InternalHelperTypeAssociation.Get(typeId);
            helper.RemoveComponentBoxed(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTypeId(object component)
        {
            var type = component.GetType();
            return GetTypeId(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTypeId(Type componentType)
        {
            var helper = InternalHelperTypeAssociation.Get(componentType);
            return helper.GetTypeInfo().id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetComponentType(int typeId)
        {
            var helper = InternalHelperTypeAssociation.Get(typeId);
            return helper.GetComponentType();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetTypeHash(Type componentType)
        {
            var helper = InternalHelperTypeAssociation.Get(componentType);
            return helper.GetTypeInfo().hash.GetValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TypeInfo GetTypeInfo(Type componentType)
        {
            var helper = InternalHelperTypeAssociation.Get(componentType);
            return helper.GetTypeInfo();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TypeInfo GetTypeInfo(int typeId)
        {
            var helper = InternalHelperTypeAssociation.Get(typeId);
            return helper.GetTypeInfo();
        }
#if MORPEH_BURST
        /// <summary>
        /// This is a reinterpret cast, ensure that the memory layout of the source component type corresponds to the target type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnmanagedStash<T> CreateUnmanagedStashDangerous<T>(this World world, Type componentType) where T : unmanaged
        {
            var helper = InternalHelperTypeAssociation.Get(componentType);
            return helper.CreateUnmanagedStash<T>(world);
        }

        /// <summary>
        /// This is a reinterpret cast, ensure that the memory layout of the source component type corresponds to the target type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnmanagedStash<T> CreateUnmanagedStashDangerous<T>(this World world, int typeId) where T : unmanaged
        {
            var helper = InternalHelperTypeAssociation.Get(typeId);
            return helper.CreateUnmanagedStash<T>(world);
        }
#endif
    }
}
