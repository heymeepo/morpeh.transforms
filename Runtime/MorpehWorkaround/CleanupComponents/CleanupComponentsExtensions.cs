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
            entity.world.ThreadSafetyCheck();

            Span<long> ids = stackalloc long[entity.components.count];
            Span<long> cleanupIds = stackalloc long[entity.components.count];
            Span<int> cleanupOffsets = stackalloc int[entity.components.count];
            int idsCount = 0;
            int cleanupIdsCount = 0;

            if (entity.currentArchetypeLength > 0)
            {
                foreach (var offset in entity.components)
                {
                    var typeDefinition = CommonTypeIdentifier.offsetTypeAssociation[offset];

                    if (CleanupComponentsHelper.IsCleanupComponent(ref typeDefinition))
                    {
                        cleanupIds[cleanupIdsCount] = typeDefinition.id;
                        cleanupOffsets[cleanupIdsCount] = typeDefinition.offset;
                        cleanupIdsCount++;
                    }
                    else
                    {
                        ids[idsCount++] = typeDefinition.id;
                    }
                }

                for (int i = 0; i < idsCount; i++)
                {
                    var stash = entity.world.stashes.GetValueByKey(cleanupIds[i]);
                    stash.Clean(entity);
                }
            }

            if (cleanupIdsCount > 0)
            {
                if (entity.previousArchetypeLength == 0)
                {
                    entity.previousArchetype = entity.currentArchetype;
                    entity.previousArchetypeLength = entity.currentArchetypeLength;
                }

                entity.currentArchetype = 0;
                entity.currentArchetypeLength = 0;
                entity.components.Clear();

                for (int i = 0; i < cleanupIdsCount; i++)
                {
                    entity.currentArchetype ^= cleanupIds[i];
                    entity.currentArchetypeLength++;
                    entity.components.Set(cleanupOffsets[i]);
                }

                entity.world.dirtyEntities.Set(entity.entityId.id);
                entity.isDirty = true;
            }
            else
            {
                if (entity.previousArchetypeLength > 0)
                {
                    entity.world.archetypes.GetValueByKey(entity.previousArchetype)?.Remove(entity);
                }
                else
                {
                    entity.world.archetypes.GetValueByKey(entity.currentArchetype)?.Remove(entity);
                }

                entity.world.ApplyRemoveEntity(entity.entityId.id);
                entity.world.dirtyEntities.Unset(entity.entityId.id);
                entity.DisposeFast();
            }
        }

        public static void WarmupCleanupComponents() => CleanupComponentsHelper.Load();
    }
}
