using Scellecs.Morpeh.Collections;

namespace Scellecs.Morpeh.Workaround
{
    public unsafe static class NativeBitMapExtensions
    {
        public static NativeBitMap AsNative(this BitMap bitmap)
        {
            var nativeBitmap = new NativeBitMap();

            fixed (int* countPtr = &bitmap.count)
            fixed (int* lengthPtr = &bitmap.length)
            fixed (int* capacityPtr = &bitmap.capacity)
            fixed (int* capacityMinusOnePtr = &bitmap.capacityMinusOne)
            fixed (int* lastIndexPtr = &bitmap.lastIndex)
            { 
                nativeBitmap.countPtr = countPtr;
                nativeBitmap.lengthPtr = lengthPtr;
                nativeBitmap.capacityPtr = capacityPtr;
                nativeBitmap.capacityMinusOnePtr = capacityMinusOnePtr;
                nativeBitmap.lastIndexPtr = lastIndexPtr;
                nativeBitmap.bucketsPtr = bitmap.buckets.ptr;
                nativeBitmap.dataPtr = bitmap.data.ptr;
                nativeBitmap.slotsPtr = bitmap.slots.ptr;
            }

            return nativeBitmap;
        }
    }
}
