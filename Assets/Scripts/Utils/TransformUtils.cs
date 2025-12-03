using UnityEngine;

namespace Stickman3D
{
    public static class TransformUtils
    {
        /// <summary>
        /// Gets the local matrix of the transform, relative to its parent (or, if no parent, the world).
        /// </summary>
        public static Matrix4x4 GetLocalMatrix(this Transform transform)
            => Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
    }
}
