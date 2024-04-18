using Scellecs.Morpeh.Collections;
using System;
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh.Workaround
{
    public static class CleanupComponentsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveAllExceptCleanupComponents(Entity entity)
        {
            var world = entity.GetWorld();
            world.ThreadSafetyCheck();

            if (world.IsDisposed(entity))
            {
#if MORPEH_DEBUG
                MLogger.LogError($"You're trying to dispose disposed entity {entity}.");
#endif
                return;
            }

            ref var entityData = ref world.entities[entity.Id];

            int totalComponentsCount = entityData.currentArchetype.components.length + entityData.addedComponentsCount;
            Span<int> idsToRemove = stackalloc int[totalComponentsCount];
            int counter = 0;

            if (entityData.currentArchetype != null)
            {
                foreach (var typeId in entityData.currentArchetype.components)
                {
                    if (CleanupComponentsHelper.IsCleanupComponent(typeId) == false)
                    {
                        idsToRemove[counter++] = typeId;
                    }
                }
            }

            for (var i = 0; i < entityData.addedComponentsCount; i++)
            {
                var typeId = entityData.addedComponents[i];

                if (CleanupComponentsHelper.IsCleanupComponent(typeId) == false)
                {
                    idsToRemove[counter++] = typeId;
                }
            }

            if (counter != totalComponentsCount)
            {
                for (var i = 0; i < counter; i++)
                {
                    var typeId = idsToRemove[i];
                    world.GetExistingStash(typeId)?.Remove(entity);
                }
            }
            else
            {
                world.RemoveEntity(entity);
            }
        }
    }
}
