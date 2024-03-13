#if MORPEH_BURST
using Unity.Collections.LowLevel.Unsafe;

namespace Scellecs.Morpeh.Workaround
{
    public unsafe struct NativeBitMap
    {
        public const int BITS_PER_BYTE = 8;
        public const int BITS_PER_FIELD = BITS_PER_BYTE * sizeof(int);
        public const int BITS_PER_FIELD_SHIFT = 5;

        [NativeDisableUnsafePtrRestriction]
        public int* countPtr;

        [NativeDisableUnsafePtrRestriction]
        public int* lengthPtr;

        [NativeDisableUnsafePtrRestriction]
        public int* capacityPtr;

        [NativeDisableUnsafePtrRestriction]
        public int* capacityMinusOnePtr;

        [NativeDisableUnsafePtrRestriction]
        public int* lastIndexPtr;

        [NativeDisableUnsafePtrRestriction]
        public int* bucketsPtr;

        [NativeDisableUnsafePtrRestriction] 
        public int* dataPtr;

        [NativeDisableUnsafePtrRestriction]
        public int* slotsPtr;
    }
}
#endif
