using Scellecs.Morpeh.Collections;

namespace Scellecs.Morpeh.Workaround
{
    public struct ArchetypeIdsCollection
    {
        public int Count => filter.archetypesLength;

        private Filter filter;

        public ArchetypeIdsCollection(Filter filter) => this.filter = filter;

        public Enumerator GetEnumerator() => new Enumerator(filter);

        public struct Enumerator
        {
            private FastList<Archetype>.Enumerator archetypesEnumerator;

            internal Enumerator(Filter filter) => archetypesEnumerator = filter.archetypes.GetEnumerator();

            public bool MoveNext() => archetypesEnumerator.MoveNext();

            public ArchetypeId Current => archetypesEnumerator.Current.AsArchetypeId();
        }
    }
}
