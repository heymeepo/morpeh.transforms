#if MORPEH_BURST
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh.Workaround
{
    public static unsafe class NativeStashMapExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TryGetIndex(this ref NativeStashMap hashMap, in int key)
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
