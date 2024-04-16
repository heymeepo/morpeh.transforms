namespace Scellecs.Morpeh.Workaround
{
    public struct ArchetypeId
    {
        public long ID => archetype.id;

        internal Archetype archetype;

        internal ArchetypeId(Archetype archetype) => this.archetype = archetype;
    }
}
