#if MORPEH_BURST
using Scellecs.Morpeh.Native;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
#endif

using Scellecs.Morpeh.Collections;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Scellecs.Morpeh.Workaround
{
    public static unsafe class MorpehCollectionsExtensions
    {
#if MORPEH_BURST
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeStash<TNative> AsNative<TNative>(this Stash<TNative> stash) where TNative : unmanaged, IComponent {
            var slotMap = stash.map;
            var nativeIntSlotMap = new NativeIntSlotMap();
            var nativeStash = default(NativeStash<TNative>);

            fixed (int* capacityMinusOnePtr = &slotMap.capacityMinusOne)
            fixed (TNative* dataPtr = &stash.data[0]) 
            fixed (TNative* emptyPtr = &stash.empty) {
                nativeIntSlotMap.capacityMinusOnePtr = capacityMinusOnePtr;
                nativeIntSlotMap.buckets = slotMap.buckets.ptr;
                nativeIntSlotMap.slots = slotMap.slots.ptr;
                nativeStash.data = dataPtr;
                nativeStash.empty = emptyPtr;
            }

            nativeStash.map = nativeIntSlotMap;
            nativeStash.world = stash.world.AsNative();

            return nativeStash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> AsNativeArrayCopy<T>(this FastList<T> list, Allocator allocator) where T : unmanaged
        {
            if (list.length > 0)
            {
                var nativeArray = new NativeArray<T>(list.length, allocator);

                fixed (T* sourcePtr = &list.data[0])
                {
                    var destinationPtr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nativeArray);
                    UnsafeUtility.MemCpy(destinationPtr, sourcePtr, sizeof(T) * list.length);
                }

                return nativeArray;
            }

            return new NativeArray<T>();
        }
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(this LongHashMap<T> hashMap, long key, [CanBeNull] out T value, out int index)
        {
            var rem = key & hashMap.capacityMinusOne;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next)
            {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key)
                {
                    value = hashMap.data[i];
                    index = i;
                    return true;
                }

                next = slot.next;
            }

            value = default;
            index = -1;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(this IntHashMap<T> hashMap, int key, [CanBeNull] out T value, out int index)
        {
            var rem = key & hashMap.capacityMinusOne;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next)
            {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key)
                {
                    value = hashMap.data[i];
                    index = i;
                    return true;
                }

                next = slot.next;
            }

            value = default;
            index = -1;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetUnsafeDataPtr<T>(this IntHashMap<T> hashMap) where T : unmanaged
        {
            fixed (T* ptr = &hashMap.data[0])
            {
                return ptr;
            }
        }
    }
}
