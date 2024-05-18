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
        internal T* data;
        internal NativeIntHashMapMetadata metadata;
        internal ulong elementSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UnmanagedStash
    {
        [NativeDisableParallelForRestriction]
        [NativeDisableUnsafePtrRestriction]
        internal void* data;
        internal NativeIntHashMapMetadata metadata;
        internal ulong elementSize;
    }
}
#endif
