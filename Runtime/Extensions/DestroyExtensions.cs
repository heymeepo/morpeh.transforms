using Scellecs.Morpeh.Workaround;
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh.Transforms
{
    public static class DestroyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(this Entity entity) => CleanupComponentsExtensions.RemoveAllExceptCleanupComponents(entity);
#pragma warning disable 0618
        public static void DestroyHierarchy(this Entity entity)
        {
            if (entity.Has<Child>())
            {
                var children = entity.GetComponent<Child>();

                for (int i = 0; i < children.Value.Length; i++)
                {
                    DestroyHierarchy(children.Value[i]);
                }
            }

            entity.Destroy();
        }
#pragma warning restore 0618
    }
}
