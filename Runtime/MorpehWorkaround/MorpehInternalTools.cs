using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using System;
using System.Runtime.CompilerServices;

namespace Prototypes.Core.ECS.MorpehWorkaround
{
    public static class MorpehInternalTools
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetComponentBoxed(this Entity entity, object component)
        {
            var type = component.GetType();
            var helper = InternalHelperTypeAssociation.Get(type);
            helper.SetComponentBoxed(entity, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetComponentBoxed(this Entity entity, object component, ComponentTypeId typeId)
        {
            var helper = InternalHelperTypeAssociation.GetFast(typeId);
            helper.SetComponentBoxed(entity, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponentBoxed(this Entity entity, object component)
        {
            var type = component.GetType();
            var helper = InternalHelperTypeAssociation.Get(type);
            helper.RemoveComponentBoxed(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponentByType(this Entity entity, Type componentType)
        {
            var helper = InternalHelperTypeAssociation.Get(componentType);
            helper.RemoveComponentBoxed(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponentByTypeId(this Entity entity, ComponentTypeId typeId)
        {
            var helper = InternalHelperTypeAssociation.GetFast(typeId);
            helper.RemoveComponentBoxed(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentTypeId GetTypeId(object component)
        {
            var type = component.GetType();
            return GetTypeId(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentTypeId GetTypeId(Type componentType)
        {
            var helper = InternalHelperTypeAssociation.Get(componentType);
            return helper.id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLength<T>(this Stash<T> stash) where T : struct, IComponent
        {
            stash.world.ThreadSafetyCheck();
            return stash.components.length - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity GetEntity(World world, in EntityId entityId)
        {
            world.ThreadSafetyCheck();
            return world.entities[entityId.id];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static World GetWorldFromEntity(Entity entity) => entity.world;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveAllExceptCleanupComponents(Entity entity)
        {
            entity.world.ThreadSafetyCheck();

            unsafe
            {
                bool disposing = true;

                if (entity.currentArchetypeLength > 0)
                {
                    long* ids = stackalloc long[entity.components.count]; 
                    int offsetsCount = 0;
                    int cleanupOffsetsCount = 0;

                    foreach (var offset in entity.components)
                    {
                        var typeDefinition = CommonTypeIdentifier.offsetTypeAssociation[offset];

                        if (CleanupComponentsHelper.IsCleanupComponent(ref typeDefinition))
                        {
                            cleanupOffsetsCount++;
                        }
                        else
                        {
                            ids[offsetsCount++] = typeDefinition.id;
                        }
                    }

                    if (cleanupOffsetsCount > 0)
                    {
                        for (int i = 0; i < offsetsCount; i++)
                        {
                            var stash = Stash.stashes.data[entity.world.stashes.GetValueByKey(ids[i])];
                            stash.Remove(entity);
                        }

                        disposing = false;
                    }
                    else
                    {
                        for (int i = 0; i < offsetsCount; i++)
                        {
                            var stash = Stash.stashes.data[entity.world.stashes.GetValueByKey(ids[i])];
                            stash.Clean(entity);
                        }
                    }
                }

                if (disposing)
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
        }

        public static void WarmupCleanupComponents() => CleanupComponentsHelper.Load();
#if MORPEH_BURST
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeUnmanagedStash<TUnmanaged> CreateUnmanagedStashDangerous<TUnmanaged>(this World world, Type componentType) where TUnmanaged : unmanaged
        {
            var helper = InternalHelperTypeAssociation.Get(componentType);
            return helper.CreateUnmanagedStash<TUnmanaged>(world);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeUnmanagedStash<TUnmanaged> CreateUnmanagedStashDangerous<TUnmanaged>(this World world, ComponentTypeId typeId) where TUnmanaged : unmanaged
        {
            var helper = InternalHelperTypeAssociation.GetFast(typeId);
            return helper.CreateUnmanagedStash<TUnmanaged>(world);
        }
#endif
    }
}
