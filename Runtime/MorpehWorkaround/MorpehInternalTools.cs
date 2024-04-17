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
        public static void SetComponentBoxed(this Entity entity, object component, out long typeId)
        {
            var type = component.GetType();
            var helper = InternalHelperTypeAssociation.Get(type);
            typeId = helper.GetTypeInfo().id;
            helper.SetComponentBoxed(entity, component);
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
        public static void RemoveComponentByTypeId(this Entity entity, int id)
        {
            var helper = InternalHelperTypeAssociation.Get(id);
            helper.RemoveComponentBoxed(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetTypeId(object component)
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
        internal static TypeInfo GetTypeInfo(Type componentType)
        {
            var helper = InternalHelperTypeAssociation.Get(componentType);
            return helper.GetTypeInfo();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TypeInfo GetTypeInfo(int id)
        {
            var helper = InternalHelperTypeAssociation.Get(id);
            return helper.GetTypeInfo();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Type GetComponentType(int id)
        {
            var helper = InternalHelperTypeAssociation.Get(id);
            return helper.GetComponentType();
        }
#if MORPEH_BURST
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeUnmanagedStash<TUnmanaged> CreateUnmanagedStashDangerous<TUnmanaged>(this World world, Type componentType) where TUnmanaged : unmanaged
        {
            var helper = InternalHelperTypeAssociation.Get(componentType);
            return helper.CreateUnmanagedStash<TUnmanaged>(world);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeUnmanagedStash<TUnmanaged> CreateUnmanagedStashDangerous<TUnmanaged>(this World world, int id) where TUnmanaged : unmanaged
        {
            var helper = InternalHelperTypeAssociation.Get(id);
            return helper.CreateUnmanagedStash<TUnmanaged>(world);
        }
#endif
    }
}
