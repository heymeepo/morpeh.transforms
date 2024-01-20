using System;

namespace Scellecs.Morpeh.Transforms
{
    /// <summary>
    /// This component specifies the parent entity in a transform hierarchy.
    /// </summary>
    /// <remarks>
    /// If present, this entity's transform is implicitly specified relative to the parent's transform rather than in world-space.
    ///
    /// Add or remove this attribute to your code in order to add, change, or remove a parent/child
    /// relationship. The corresponding <see cref="Child"/> component is automatically added by the <see cref="ParentSystem"/>.
    ///
    /// When adding or modifying this component, add and update the corresponding <see cref="LocalTransform"/> component.
    /// </remarks>
    /// <seealso cref="Child"/>
    [Serializable]
    public struct Parent : IComponent
    {
        /// <summary>
        /// The parent entity.
        /// </summary>
        /// <remarks>This field must refer to a valid entity. Root level entities should not use <see cref="Entity.Null"/>;
        /// rather, they should not have the <see cref="Parent"/> component at all.</remarks>
        public EntityId Value;
    }
}
