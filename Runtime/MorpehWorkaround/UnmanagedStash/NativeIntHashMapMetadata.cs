#if MORPEH_BURST
using Scellecs.Morpeh.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh.Workaround
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeIntHashMapMetadata
    {
        [NativeDisableUnsafePtrRestriction]
        public int* lengthPtr;

        [NativeDisableUnsafePtrRestriction]
        public int* capacityPtr;

        [NativeDisableUnsafePtrRestriction]
        public int* capacityMinusOnePtr;

        [NativeDisableUnsafePtrRestriction]
        public int* lastIndexPtr;

        [NativeDisableUnsafePtrRestriction]
        public int* buckets;

        [NativeDisableUnsafePtrRestriction]
        public IntHashMapSlot* slots;
    }

    internal static unsafe class NativeIntHashMapMetadataExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TryGetIndex(this ref NativeIntHashMapMetadata hashMap, in int key)
        {
            var rem = key & *hashMap.capacityMinusOnePtr;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next)
            {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key)
                {
                    return i;
                }

                next = slot.next;
            }

            return -1;
        }
    }
}
#endif
