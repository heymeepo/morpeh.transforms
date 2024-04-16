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
    }
}

