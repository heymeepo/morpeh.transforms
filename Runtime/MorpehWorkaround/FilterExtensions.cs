using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace Scellecs.Morpeh.Workaround
{
    public static class FilterExtensions
    {
        public static FilterBuilder With(this FilterBuilder builder, long typeId)
        {
            var offset = MorpehInternalTools.GetTypeOffset(typeId);
            var current = builder;

            while (current.parent != null)
            {
                if (current.mode == Filter.Mode.Include && current.offset == offset)
                {
                    return builder;
                }
                current = current.parent;
            }

            return new FilterBuilder
            {
                parent = builder,
                world = builder.world,
                mode = Filter.Mode.Include,
                typeId = typeId,
                offset = offset,
                level = builder.level + 1,
                includeHash = builder.includeHash ^ typeId,
                excludeHash = builder.excludeHash
            };
        }

        public static FilterBuilder Without(this FilterBuilder builder, long typeId)
        {
            var offset = MorpehInternalTools.GetTypeOffset(typeId);
            var current = builder;

            while (current.parent != null)
            {
                if (current.mode == Filter.Mode.Exclude && current.offset == offset)
                {
                    return builder;
                }
                current = current.parent;
            }

            return new FilterBuilder
            {
                parent = builder,
                world = builder.world,
                mode = Filter.Mode.Exclude,
                typeId = typeId,
                level = builder.level + 1,
                includeHash = builder.includeHash,
                excludeHash = builder.excludeHash ^ typeId
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<int> AsNativeArray(this Filter filter)
        {
            int totalEntities = 0;

            for (int i = 0, len = filter.archetypes.length; i < len; i++)
            {
                totalEntities += filter.archetypes.data[i].entities.count;
            }

            var nativeArray = new NativeArray<int>(totalEntities, Allocator.TempJob);
            int position = 0;

            // Проходим по каждому архетипу и каждому чанку в архетипе
            for (int i = 0, len = filter.archetypes.length; i < len; i++)
            {
                var archetype = filter.archetypes.data[i];
                // Предполагаем, что есть способ получить доступ к сущностям в архетипе напрямую
                foreach (var entityId in archetype.entities)
                { // Перебор должен соответствовать реальной структуре данных

                    nativeArray[position++] = entityId;
                }
            }

            return nativeArray;
        }

        public static BitMap[] GetArchetypes(this Filter filter, out int length)
        {
            var array = new BitMap[filter.archetypes.length];
            length = 0;

            for (int i = 0; i < filter.archetypes.length; i++)
            {
                array[i] = filter.archetypes.data[i].entities;
                length += array[i].count;
            }

            return array;
        }
    }
}
