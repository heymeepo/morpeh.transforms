namespace Scellecs.Morpeh.Workaround
{
    public static class ArchetypeIdExtensions
    {
        internal static ArchetypeId AsArchetypeId(this Archetype archetype) => new ArchetypeId(archetype);

        public static ArchetypeIdsCollection ArchetypeIds(this Filter filter) => new ArchetypeIdsCollection(filter);

        public static ArchetypeComponentsContainer AsComponentsContainer(this ArchetypeId archetype) => new ArchetypeComponentsContainer(archetype.archetype);
    }
}
