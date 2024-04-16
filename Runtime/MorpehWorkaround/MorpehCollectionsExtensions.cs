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
        public static NativeIntHashMap<TNative> AsNative<TNative>(this IntHashMap<TNative> hashMap) where TNative : unmanaged
        {
            return NativeIntHashMapExtensions.AsNative(hashMap);
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
            for (var i = hashMap.buckets.ptr[rem] - 1; i >= 0; i = next)
            {
                ref var slot = ref hashMap.slots.ptr[i];
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
        public static void RemoveChecked(this IntFastList list, int value)
        {
            if (list.length > 0)
            {
                int index = list.IndexOf(value);

                if (index >= 0)
                {
                    list.RemoveAt(index);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSwapBackChecked(this IntFastList list, int value)
        {
            for (int i = 0; i < list.length; i++)
            {
                if (list.data.ptr[i] == value)
                {
                    list.RemoveAtSwap(i, out _);
                    return;
                }
            }
        }
    }
}
