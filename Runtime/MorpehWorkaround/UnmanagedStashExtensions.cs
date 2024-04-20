#if MORPEH_BURST
using Scellecs.Morpeh.Native;
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh.Workaround
{
    public static class UnmanagedStashExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T>(this ref UnmanagedStash<T> stash, in Entity entity) where T : unmanaged
        {
            return ref stash.reinterpretedComponents.GetValueRefByKey(entity.Id);
        }
    }
}
#endif
