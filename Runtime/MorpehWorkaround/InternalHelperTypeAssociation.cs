﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace Scellecs.Morpeh.Workaround
{
    internal static class InternalHelperTypeAssociation
    {
        private static Dictionary<Type, InternalAPIHelper> typeAssociation = new Dictionary<Type, InternalAPIHelper>();
        private static Dictionary<int, InternalAPIHelper> idTypeAssociation = new Dictionary<int, InternalAPIHelper>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static InternalAPIHelper Get(Type type)
        {
            if (typeAssociation.ContainsKey(type) == false)
            {
                if ((typeof(IComponent).IsAssignableFrom(type) && type.IsValueType) == false)
                {
                    throw new ArgumentException($"The specified type {type} is not valid. Please ensure that the type you are trying to assign is a value type and implements IComponent interface.");
                }

                var typeId = typeof(InternalAPIHelper<>).MakeGenericType(type);
                var method = typeId.GetMethod("Warmup", BindingFlags.Static | BindingFlags.NonPublic);
                method.Invoke(null, null);
            }

            return typeAssociation[type];
        }

        internal static InternalAPIHelper Get(int typeId)
        {
            if (idTypeAssociation.TryGetValue(typeId, out var helper))
            {
                return helper;
            }
            else if(ComponentId.TryGet(typeId, out var type))
            {
                return Get(type);
            }

            throw new ArgumentException("Invalid TypeId!");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Set<T>(InternalAPIHelper<T> helper) where T : unmanaged, IComponent
        {
            ComponentId<T>.Warmup();
            var info = ComponentId<T>.info;
            typeAssociation.Add(typeof(T), helper);
            idTypeAssociation.Add(info.id, helper);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Cleanup()
        {
            typeAssociation.Clear();
            idTypeAssociation.Clear();
        }
    }

    internal abstract class InternalAPIHelper
    {
        internal abstract Type GetComponentType();

        internal abstract TypeInfo GetTypeInfo();

        internal abstract void SetComponentBoxed(Entity entity, object component);

        internal abstract void RemoveComponentBoxed(Entity entity);
#if MORPEH_BURST
        internal abstract UnmanagedStash<TUnmanaged> CreateUnmanagedStash<TUnmanaged>(World world) where TUnmanaged : unmanaged;

        internal abstract UnmanagedStash CreateUnmanagedStash(World world);
#endif
    }

    internal sealed class InternalAPIHelper<T> : InternalAPIHelper where T : unmanaged, IComponent
    {
        private InternalAPIHelper() { }

        [Preserve]
        private static void Warmup() => InternalHelperTypeAssociation.Set(new InternalAPIHelper<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override Type GetComponentType() => typeof(T);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override TypeInfo GetTypeInfo() => ComponentId<T>.info;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void SetComponentBoxed(Entity entity, object component) => entity.GetWorld().GetStash<T>().Set(entity, (T)component);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void RemoveComponentBoxed(Entity entity) => entity.GetWorld().GetStash<T>().Remove(entity);
#if MORPEH_BURST
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe override UnmanagedStash<TUnmanaged> CreateUnmanagedStash<TUnmanaged>(World world)
        {
            var stash = world.GetStash<T>();
            var hashMap = stash.map;
            var hashMapMetadata = new NativeIntHashMapMetadata();
            var unmanagedStash = new UnmanagedStash<TUnmanaged>() { elementSize = sizeof(T) };

            fixed (int* lengthPtr = &hashMap.length)
            fixed (int* capacityPtr = &hashMap.capacity)
            fixed (int* capacityMinusOnePtr = &hashMap.capacityMinusOne)
            fixed (int* lastIndexPtr = &hashMap.lastIndex)
            fixed (int* freeIndexPtr = &hashMap.freeIndex)
            fixed (void* dataPtr = &stash.data[0])
            {
                hashMapMetadata.lengthPtr = lengthPtr;
                hashMapMetadata.capacityPtr = capacityPtr;
                hashMapMetadata.capacityMinusOnePtr = capacityMinusOnePtr;
                hashMapMetadata.lastIndexPtr = lastIndexPtr;
                hashMapMetadata.freeIndexPtr = freeIndexPtr;
                hashMapMetadata.buckets = hashMap.buckets.ptr;
                hashMapMetadata.slots = hashMap.slots.ptr;
                unmanagedStash.data = (TUnmanaged*)dataPtr;
            }

            unmanagedStash.metadata = hashMapMetadata;
            unmanagedStash.elementSize = sizeof(T);
            return unmanagedStash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe override UnmanagedStash CreateUnmanagedStash(World world)
        {
            var stash = world.GetStash<T>();
            var hashMap = stash.map;
            var hashMapMetadata = new NativeIntHashMapMetadata();
            var unmanagedStash = new UnmanagedStash();

            fixed (int* lengthPtr = &hashMap.length)
            fixed (int* capacityPtr = &hashMap.capacity)
            fixed (int* capacityMinusOnePtr = &hashMap.capacityMinusOne)
            fixed (int* lastIndexPtr = &hashMap.lastIndex)
            fixed (int* freeIndexPtr = &hashMap.freeIndex)
            fixed (void* dataPtr = &stash.data[0])
            {
                hashMapMetadata.lengthPtr = lengthPtr;
                hashMapMetadata.capacityPtr = capacityPtr;
                hashMapMetadata.capacityMinusOnePtr = capacityMinusOnePtr;
                hashMapMetadata.lastIndexPtr = lastIndexPtr;
                hashMapMetadata.freeIndexPtr = freeIndexPtr;
                hashMapMetadata.buckets = hashMap.buckets.ptr;
                hashMapMetadata.slots = hashMap.slots.ptr;
                unmanagedStash.data = dataPtr;
            }

            unmanagedStash.metadata = hashMapMetadata;
            unmanagedStash.elementSize = sizeof(T);
            return unmanagedStash;
        }
#endif
    }
}
