using Scellecs.Morpeh.Collections;
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh.Workaround
{
    public struct ArchetypeComponentsContainer
    {
        public readonly long id;

        internal SortedBitMap components;

        internal ArchetypeComponentsContainer(Archetype archetype)
        {
            components = ExtractComponents(archetype);
            id = archetype.id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(int componentOffset) => components.Get(componentOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SortedBitMap.Enumerator GetEnumerator() => components.GetEnumerator();

        private static SortedBitMap ExtractComponents(Archetype archetype)
        {
            var bitmapEnumerator = archetype.entities.GetEnumerator();
            var world = archetype.world;

            if (bitmapEnumerator.MoveNext())
            {
                var entityId = bitmapEnumerator.Current;
                var gen = world.entitiesGens[entityId];

                if (world.TryGetEntity(new EntityId(entityId, gen), out var entity))
                {
                    return entity.components;
                }
            }

            return null;
        }
    }
}
