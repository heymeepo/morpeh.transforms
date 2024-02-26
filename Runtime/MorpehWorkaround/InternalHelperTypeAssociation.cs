using Scellecs.Morpeh;
#if MORPEH_BURST
using Scellecs.Morpeh.Native;
#endif
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace Prototypes.Core.ECS.MorpehWorkaround
{
    internal static class InternalHelperTypeAssociation
    {
        private static Dictionary<Type, InternalAPIHelper> typeAssociation = new Dictionary<Type, InternalAPIHelper>();
        private static Dictionary<long, InternalAPIHelper> idTypeAssociation = new Dictionary<long, InternalAPIHelper>();

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

        internal static InternalAPIHelper Get(long typeId)
        {
            if (idTypeAssociation.TryGetValue(typeId, out var helper))
            {
                return helper;
            }

            throw new ArgumentException("Invalid TypeId!");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Set<T>(InternalAPIHelper<T> helper) where T : unmanaged, IComponent
        {
            TypeIdentifier<T>.Warmup();
            var info = TypeIdentifier<T>.info;
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
        internal abstract CommonTypeIdentifier.TypeInfo GetTypeInfo();

        internal abstract void SetComponentBoxed(Entity entity, object component);

        internal abstract void RemoveComponentBoxed(Entity entity);
#if MORPEH_BURST
        internal abstract NativeUnmanagedStash<TUnmanaged> CreateUnmanagedStash<TUnmanaged>(World world) where TUnmanaged : unmanaged;
#endif
    }

    internal sealed class InternalAPIHelper<T> : InternalAPIHelper where T : unmanaged, IComponent
    {
        private InternalAPIHelper() { }

        [Preserve]
        private static void Warmup() => InternalHelperTypeAssociation.Set(new InternalAPIHelper<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override CommonTypeIdentifier.TypeInfo GetTypeInfo() => TypeIdentifier<T>.info;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void SetComponentBoxed(Entity entity, object component) => entity.world.GetStash<T>().Set(entity, (T)component);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void RemoveComponentBoxed(Entity entity) => entity.world.GetStash<T>().Remove(entity);
#if MORPEH_BURST
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe override NativeUnmanagedStash<TUnmanaged> CreateUnmanagedStash<TUnmanaged>(World world)
        {
            var stash = world.GetStash<T>();
            var hashMap = stash.components;
            var nativeIntHashMap = new NativeIntHashMap<TUnmanaged>();

            fixed (int* lengthPtr = &hashMap.length)
            fixed (int* capacityPtr = &hashMap.capacity)
            fixed (int* capacityMinusOnePtr = &hashMap.capacityMinusOne)
            fixed (int* lastIndexPtr = &hashMap.lastIndex)
            fixed (int* freeIndexPtr = &hashMap.freeIndex)
            fixed (void* dataPtr = &hashMap.data[0])
            {
                nativeIntHashMap.lengthPtr = lengthPtr;
                nativeIntHashMap.capacityPtr = capacityPtr;
                nativeIntHashMap.capacityMinusOnePtr = capacityMinusOnePtr;
                nativeIntHashMap.lastIndexPtr = lastIndexPtr;
                nativeIntHashMap.freeIndexPtr = freeIndexPtr;
                nativeIntHashMap.data = (TUnmanaged*)dataPtr;
                nativeIntHashMap.buckets = hashMap.buckets.ptr;
                nativeIntHashMap.slots = hashMap.slots.ptr;
            }

            return new NativeUnmanagedStash<TUnmanaged>()
            {
                reinterpretedComponents = nativeIntHashMap,
                world = world.AsNative()
            };
        }
#endif
    }
}
