namespace Scellecs.Morpeh.Workaround
{
    public static class FilterWorkaroundExtensions
    {
        public static FilterBuilder With(this FilterBuilder builder, int typeId)
        {
            var info = MorpehInternalTools.GetTypeInfo(typeId);
            var current = builder;

            while (current.parent != null)
            {
                if (current.typeInfo.id == info.id && current.mode == Filter.Mode.Include)
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
                typeInfo = info,
                level = builder.level + 1,
                includeHash = builder.includeHash.Combine(info.hash),
                excludeHash = builder.excludeHash
            };
        }

        public static FilterBuilder Without(this FilterBuilder builder, int typeId)
        {
            var info = MorpehInternalTools.GetTypeInfo(typeId);
            var current = builder;

            while (current.parent != null)
            {
                if (current.typeInfo.id == info.id && current.mode == Filter.Mode.Exclude)
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
                typeInfo = info,
                level = builder.level + 1,
                includeHash = builder.includeHash,
                excludeHash = builder.excludeHash.Combine(info.hash)
            };
        }

        public static InternalFilterInfo GetInternalFilterInfoDangerous(Filter filter)
        {
            return new InternalFilterInfo()
            {
                archetypes = filter.archetypes,
                archetypesLength = filter.archetypesLength,
                archetypeHashes = filter.archetypeHashes
            };
        }
    }
}

