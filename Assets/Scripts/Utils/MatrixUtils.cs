using UnityEngine;

namespace Stickman3D
{
    public static class MatrixUtils
    {
        public static Matrix4x4 Lerp(Matrix4x4 from, Matrix4x4 to, float t)
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
    }
}
