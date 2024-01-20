using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Scellecs.Morpeh.Transforms
{
    /// <summary>
    /// Contains a buffer of all elements which have assigned this entity as their <see cref="Parent"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="ParentSystem"/> automatically adds and manages this component and its contents. You can read this
    /// list, but you shouldn't add or remove buffer elements.
    ///
    /// When an entity with this component is destroyed, the <see cref="ParentSystem"/> will automatically remove the
    /// <see cref="Parent"/> components from each child entity.
    /// </remarks>
    [Serializable]
    public struct Child : IComponent, IDisposable
    {
        /// <summary>
        /// A child entity
        /// </summary>
        [NativeDisableContainerSafetyRestriction] public NativeList<EntityId> Value;

        public void Dispose()
        {
            if (Value.IsCreated)
            {
                Value.Dispose();
            }
        }
    }
}
