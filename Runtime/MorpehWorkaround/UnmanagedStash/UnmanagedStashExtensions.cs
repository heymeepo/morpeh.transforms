#if MORPEH_BURST
using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Scellecs.Morpeh.Workaround
{
    public static unsafe class UnmanagedStashExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T>(this ref UnmanagedStash<T> stash, in Entity entity) where T : unmanaged
        {
            var idx = stash.metadata.TryGetIndex(entity.Id);
            return ref stash.data[idx];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetElementSize(this ref UnmanagedStash stash)
        {
            return (int)stash.elementSize;
        }

        /// <summary>
        /// This is a reinterpret cast, ensure that the memory layout of the source component type corresponds to the target type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref UnmanagedStash<T> Convert<T>(this ref UnmanagedStash stash) where T : unmanaged
        {
            return ref *(UnmanagedStash<T>*)UnsafeUtility.AddressOf(ref stash);
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

        /// <summary>
        /// Create an untyped stash with a void pointer to data can be converted to a typed one.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnmanagedStash CreateUnmanagedStashDangerous(this World world, Type componentType)
        {
            var helper = InternalHelperTypeAssociation.Get(componentType);
            return helper.CreateUnmanagedStash(world);
        }

        /// <summary>
        /// Create an untyped stash with a void pointer to data can be converted to a typed one.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnmanagedStash CreateUnmanagedStashDangerous(this World world, int typeId)
        {
            var helper = InternalHelperTypeAssociation.Get(typeId);
            return helper.CreateUnmanagedStash(world);
        }
    }
}
#endif
