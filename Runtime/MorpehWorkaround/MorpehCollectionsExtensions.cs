using Scellecs.Morpeh.Collections;
#if MORPEH_BURST
using Scellecs.Morpeh.Native;
#endif
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh.Workaround
{
    public static class MorpehCollectionsExtensions
    {
#if MORPEH_BURST
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeIntHashMap<TNative> AsNative<TNative>(this IntHashMap<TNative> hashMap) where TNative : unmanaged
        {
            return NativeIntHashMapExtensions.AsNative(hashMap);
        }
#endif
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
        public static unsafe void RemoveSwapBackChecked(this IntFastList list, int value)
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
