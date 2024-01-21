using Prototypes.Core.ECS.MorpehWorkaround;
using System;

namespace Scellecs.Morpeh.Transforms
{
    /// <summary>
    /// Utility component used by the <see cref="ParentSystem"/> to detect changes to an entity's <see cref="Parent"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="ParentSystem"/> automatically adds and manages this component.  You shouldn't
    /// add, remove, or modify it in your code.
    /// </remarks>
    [Serializable]
    public struct PreviousParent : ICleanupComponent
    {
        /// <summary>
        /// The previous parent entity
        /// </summary>
        public EntityId Value;
    }
}
