using Scellecs.Morpeh.Workaround;
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh.Transforms
{
    public static class DestroyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(this Entity entity) => CleanupComponentsExtensions.RemoveAllExceptCleanupComponents(entity);

        public static void DestroyHierarchy(this Entity entity)
        {
            if (entity.Has<Child>())
            {
                var children = entity.GetComponent<Child>();
                var world = MorpehInternalTools.GetWorldFromEntity(entity);

                for (int i = 0; i < children.Value.Length; i++)
                {
                    if (world.TryGetEntity(children.Value[i], out var child))
                    {
                        DestroyHierarchy(child);
                    }
                }
            }

            entity.Destroy();
        }
    }
}
