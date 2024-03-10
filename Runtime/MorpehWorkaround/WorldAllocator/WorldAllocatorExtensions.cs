using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Scellecs.Morpeh.Workaround.WorldAllocator
{
    public static unsafe class WorldAllocatorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static WorldRewindableAllocator GetWorldAllocator(World world) => WorldAllocatorPlugin.rwdAllocators[world.identifier];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WorldRewindableAllocator RewindableAllocator(this World world) => GetWorldAllocator(world);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Allocate(this WorldRewindableAllocator allocator, int sizeOf, int alignOf, int itemsCount, NativeArrayOptions options)
        {
            allocator.world.ThreadSafetyCheck();
            var ptr = allocator.rwdAllocator.Allocator.Allocate(sizeOf, alignOf, itemsCount);

            if (options == NativeArrayOptions.ClearMemory)
            {
                UnsafeUtility.MemClear(ptr, sizeOf * itemsCount);
            }

            return ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Allocate<T>(this WorldRewindableAllocator allocator, int itemsCount, NativeArrayOptions options) where T : unmanaged
        {
            return (T*)Allocate(allocator, UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), itemsCount, options);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> AllocateNativeArray<T>(this WorldRewindableAllocator allocator, int itemsCount, NativeArrayOptions options) where T : unmanaged
        {
            allocator.world.ThreadSafetyCheck();
            return CollectionHelper.CreateNativeArray<T, RewindableAllocator>(itemsCount, ref allocator.rwdAllocator.Allocator, options);
        }
    }
}
