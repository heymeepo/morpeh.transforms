#if MORPEH_BURST
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using System.Runtime.InteropServices;

namespace Scellecs.Morpeh.Workaround
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UnmanagedStash<T> where T : unmanaged
    {
        [NativeDisableParallelForRestriction]
        [NativeDisableUnsafePtrRestriction]
        public T* data;
        public NativeStashMap stashMap;
        public ulong elementSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UnmanagedStash
    {
        [NativeDisableParallelForRestriction]
        [NativeDisableUnsafePtrRestriction]
        public void* data;
        public NativeStashMap stashMap;
        public ulong elementSize;
    }
}
#endif
