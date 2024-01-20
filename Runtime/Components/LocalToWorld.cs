using System;
using Unity.Mathematics;

namespace Scellecs.Morpeh.Transforms
{
    /// <summary>
    /// The local-to-world transformation matrix for an entity
    /// </summary>
    /// <remarks>
    /// This matrix is primarily intended for consumption by the rendering systems.
    ///
    /// The matrix value is generally updated automatically by <see cref="LocalToWorldSystem"/> based on the entity's
    /// <see cref="LocalTransform"/>.
    ///
    /// This component value may be out of date or invalid while the <see cref="SimulationSystemGroup"/> is running; it
    /// is only updated when the <see cref="TransformSystemGroup"/> runs).
    /// It may also contain additional offsets applied for graphical smoothing purposes.
    /// Therefore, while the <see cref="LocalToWorld"/> component may be useful as a fast approximation of an entity's
    /// world-space transformation when its latency is acceptable, it should not be relied one when an accurate,
    /// up-to-date world transform is needed for simulation purposes. In those cases, use the
    /// <see cref="TransformsHelpers.ComputeWorldTransformMatrix"/> method.
    ///
    /// If a system writes to this component directly outside of the Entities transform systems using a <see cref="WriteGroupAttribute"/>,
    /// <see cref="LocalToWorldSystem"/> will not overwrite this entity's matrix. In this case, the writing system is
    /// also responsible for applying the entity's <see cref="PostTransformMatrix"/> component (if present).
    /// </remarks>
    [Serializable]
    public struct LocalToWorld : IComponent
    {
        /// <summary>
        /// The transformation matrix
        /// </summary>
        public float4x4 value;

        /// <summary>
        /// The "right" vector, in the entity's world-space.
        /// </summary>
        public float3 Right => new float3(value.c0.x, value.c0.y, value.c0.z);

        /// <summary>
        /// The "up" vector, in the entity's world-space.
        /// </summary>
        public float3 Up => new float3(value.c1.x, value.c1.y, value.c1.z);

        /// <summary>
        /// The "forward" vector, in the entity's world-space.
        /// </summary>
        public float3 Forward => new float3(value.c2.x, value.c2.y, value.c2.z);

        /// <summary>
        /// The "entity's" position in world-space.
        /// </summary>
        public float3 Position => new float3(value.c3.x, value.c3.y, value.c3.z);

        /// <summary>
        /// The "entity's" orientation in world-space.
        /// </summary>
        public quaternion Rotation => new quaternion(math.orthonormalize(new float3x3(value)));
    }
}
