#if MORPEH_BURST
using Scellecs.Morpeh.Native;

namespace Prototypes.Core.ECS.MorpehWorkaround
{
    public struct NativeUnmanagedStash<T> where T : unmanaged
    {
        public NativeIntHashMap<T> componentsAsUnmanagedType;
        public NativeWorld world;
    }
}
#endif
