using UnityEngine;

namespace Stickman3D
{
    public static class MatrixUtils
    {
        /// <summary>
        /// Lerps two Matrix4x4 transforms by lerping their position, rotation, and scale components separately.
        /// </summary>
        public static Matrix4x4 Lerp(in Matrix4x4 from, in Matrix4x4 to, float t)
        {
            // Decompose 'from' matrix
            var fromPosition = from.GetColumn(3); // Position is in the last column
            var fromRotation = from.rotation;
            var fromScale = from.lossyScale;

            // Decompose 'to' matrix
            var toPosition = to.GetColumn(3);
            var toRotation = to.rotation;
            var toScale = to.lossyScale;

            // Lerp each component
            var interpolatedPosition = Vector3.Lerp(fromPosition, toPosition, t);
            var interpolatedRotation = Quaternion.Slerp(fromRotation, toRotation, t);
            var interpolatedScale = Vector3.Lerp(fromScale, toScale, t);

            // Recompose the new Matrix4x4
            return Matrix4x4.TRS(interpolatedPosition, interpolatedRotation, interpolatedScale);
        }

        /// <summary>
        /// Determines the approximate equivalence of two Matrix4x4s within a specified tolerance.
        /// </summary>
        public static bool IsApproximatelyEqual(in Matrix4x4 a, in Matrix4x4 b, float tolerance = 0.0001f)
        {
            for (var i = 0; i < 16; i++)
            {
                if (Mathf.Abs(a[i] - b[i]) > tolerance)
                    return false;
            }
            return true;
        }
    }
}
