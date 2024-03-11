#if MORPEH_BURST
using Scellecs.Morpeh.Native;

namespace Scellecs.Morpeh.Workaround
{
    public struct NativeUnmanagedStash<T> where T : unmanaged
    {
        public NativeIntHashMap<T> reinterpretedComponents;
        public NativeWorld world;
    }
}
#endif
