#if MORPEH_BURST
using Scellecs.Morpeh.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;

namespace Scellecs.Morpeh.Workaround
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NativeStashMap
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
}
#endif
