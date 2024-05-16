#if MORPEH_BURST
using Scellecs.Morpeh.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace Scellecs.Morpeh.Workaround
{
    public unsafe struct NativeIntHashMap
    {
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* lengthPtr;

        [NativeDisableUnsafePtrRestriction]
        public unsafe int* capacityPtr;

        [NativeDisableUnsafePtrRestriction]
        public unsafe int* capacityMinusOnePtr;

        [NativeDisableUnsafePtrRestriction]
        public unsafe int* lastIndexPtr;

        [NativeDisableUnsafePtrRestriction]
        public unsafe int* freeIndexPtr;

        [NativeDisableUnsafePtrRestriction]
        public unsafe int* buckets;

        [NativeDisableUnsafePtrRestriction]
        public unsafe IntHashMapSlot* slots;

        [NativeDisableParallelForRestriction]
        [NativeDisableUnsafePtrRestriction]
        public unsafe void* data;
    }
}
#endif
