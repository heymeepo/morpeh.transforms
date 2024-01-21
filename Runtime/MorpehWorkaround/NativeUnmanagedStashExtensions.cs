#if MORPEH_BURST
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using System.Runtime.CompilerServices;

namespace Prototypes.Core.ECS.MorpehWorkaround
{
    public static class NativeUnmanagedStashExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T>(this ref NativeUnmanagedStash<T> stash, in EntityId entityId) where T : unmanaged
        {
            return ref stash.reinterpretedComponents.GetValueRefByKey(in entityId.id);
        }
    }
}
#endif
