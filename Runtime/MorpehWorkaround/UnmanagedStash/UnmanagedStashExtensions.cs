#if MORPEH_BURST
using Scellecs.Morpeh.Native;
using System;
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh.Workaround
{
    public static class UnmanagedStashExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T>(this ref UnmanagedStash<T> stash, in Entity entity) where T : unmanaged
        {
            return ref stash.reinterpretedComponents.GetValueRefByKey(entity.Id);
        }

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
    }
}
#endif
