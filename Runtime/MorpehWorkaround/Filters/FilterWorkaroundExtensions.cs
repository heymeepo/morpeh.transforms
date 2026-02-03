using Scellecs.Morpeh.Collections;

namespace Scellecs.Morpeh.Workaround
{
    public static class FilterWorkaroundExtensions
    {
        public static FilterBuilder With(this FilterBuilder builder, int typeId)
        {
            var current = builder;
            if (current.contextVersion != current.context.version) {
                FilterBuilderReuseException.Throw();
            }
            
            var info = MorpehInternalTools.GetTypeInfo(typeId);
            
            for (var i = current.context.withCount - 1; i >= 0; i--) {
                if (current.context.with[i] == info.id)
                {
                    var type = MorpehInternalTools.GetComponentType(typeId);
                    ComponentExistsInFilterException.Throw(type);
                }
            }
            
            for (var i = current.context.withoutCount - 1; i >= 0; i--) {
                if (current.context.without[i] == info.id) {
                    var type = MorpehInternalTools.GetComponentType(typeId);
                    ComponentExistsInFilterException.Throw(type);
                }
            }

            if (current.context.withCount == current.context.with.Length) {
                ArrayHelpers.Grow(ref current.context.with, current.context.with.Length << 1);
            }
            
            current.context.with[current.context.withCount++] = info.id;
            unchecked { current.context.version++; }

            return new FilterBuilder {
                world          = current.world,
                includeHash    = current.includeHash.Combine(info.hash),
                excludeHash    = current.excludeHash,
                context        = current.context,
                contextVersion = current.context.version,
            };
        }

        public static FilterBuilder Without(this FilterBuilder builder, int typeId)
        {
            var current = builder;
            if (current.contextVersion != current.context.version) {
                FilterBuilderReuseException.Throw();
            }
            
            var info = MorpehInternalTools.GetTypeInfo(typeId);
            
            for (var i = current.context.withCount - 1; i >= 0; i--) {
                if (current.context.with[i] == info.id) {
                    var type = MorpehInternalTools.GetComponentType(typeId);
                    ComponentExistsInFilterException.Throw(type);
                }
            }
            
            for (var i = current.context.withoutCount - 1; i >= 0; i--) {
                if (current.context.without[i] == info.id) {
                    var type = MorpehInternalTools.GetComponentType(typeId);
                    ComponentExistsInFilterException.Throw(type);
                }
            }

            if (current.context.withoutCount == current.context.without.Length) {
                ArrayHelpers.Grow(ref current.context.without, current.context.without.Length << 1);
            }
            
            current.context.without[current.context.withoutCount++] = info.id;
            unchecked { current.context.version++; }
            
            return new FilterBuilder {
                world          = current.world,
                includeHash    = current.includeHash,
                excludeHash    = current.excludeHash.Combine(info.hash),
                context        = current.context,
                contextVersion = current.context.version,
            };
        }

        /// <summary>
        /// Don't modify anything inside, don't cache archetypes, use for readonly access
        /// </summary>
        public static InternalFilterInfo GetInternalFilterInfo(Filter filter)
        {
            return new InternalFilterInfo()
            {
                archetypes = filter.archetypes,
                archetypesLength = filter.archetypesLength,
                archetypesHashes = filter.archetypeHashesMap
            };
        }
    }
}

