#if MORPEH_BURST
using Scellecs.Morpeh.Native;

namespace Scellecs.Morpeh.Workaround
{
    public struct UnmanagedStash<T> where T : unmanaged
    {
        public NativeIntHashMap<T> data;
        public NativeWorld world;
    }

    public struct UnmanagedStash
    {
        public NativeIntHashMap data;
        public NativeWorld world;
        public int elementSize;
    }
}
#endif
