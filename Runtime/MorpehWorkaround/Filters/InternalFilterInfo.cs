using Scellecs.Morpeh.Collections;

namespace Scellecs.Morpeh.Workaround
{
    public struct InternalFilterInfo
    {
        public Archetype[] archetypes;
        public int archetypesLength;
        public LongHashMap<int> archetypesHashes;
    }
}

