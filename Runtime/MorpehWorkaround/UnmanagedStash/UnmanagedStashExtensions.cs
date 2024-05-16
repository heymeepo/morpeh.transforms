#if MORPEH_BURST
using Scellecs.Morpeh.Native;
using System;
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh.Workaround
{
    public static unsafe class UnmanagedStashExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T>(this ref UnmanagedStash<T> stash, in Entity entity) where T : unmanaged
        {
            return ref stash.data.GetValueRefByKey(entity.Id);
        }

        public static UnmanagedStash<T> Convert<T>(this ref UnmanagedStash stash) where T : unmanaged
        {
            var nativeIntHashMap = new NativeIntHashMap<T>()
            {
                lengthPtr = stash.data.lengthPtr,
                capacityPtr = stash.data.capacityPtr,
                capacityMinusOnePtr = stash.data.capacityMinusOnePtr,
                lastIndexPtr = stash.data.lastIndexPtr,
                freeIndexPtr = stash.data.freeIndexPtr,
                buckets = stash.data.buckets,
                slots = stash.data.slots,
                data = (T*)stash.data.data
            };

            return new UnmanagedStash<T>()
            {
                data = nativeIntHashMap,
                world = stash.world
            };
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
        public static UnmanagedStash CreateUnmanagedStashDangerous(this World world, int typeId)
        {
            var helper = InternalHelperTypeAssociation.Get(typeId);
            return helper.CreateUnmanagedStash(world);
        }
    }
}
#endif
