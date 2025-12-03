using UnityEngine;

namespace Stickman3D.Gizmos
{
    public class ScaleGizmoController : GizmoController
    {
        [SerializeField]
        private float scaleFactor = 0.5f;

        private Vector3 lastMouseWorldPosition;
        private Vector3 axisDirection;

        protected override void OnInteractionStart(Transform baseAxis, Vector3 hitPoint)
        {
            // Cache the starting world position for calculating deltas
            lastMouseWorldPosition = hitPoint;

            // Determine the axis direction in local space based on which gizmo axis is active
            if (baseAxis == XAxis)
            {
                axisDirection = SelectedObject.right;
            }
            else if (baseAxis == YAxis)
            {
                axisDirection = SelectedObject.up;
            }
            else if (baseAxis == ZAxis)
            {
                axisDirection = SelectedObject.forward;
            }
        }

        protected override void OnInteractionUpdate()
        {
            // Raycast to get current mouse world position
            if (!Physics.Raycast(MouseRay, out var hit, Mathf.Infinity, GizmoLayer))
                return;

            // Calculate delta movement in world space
            var currentWorldPosition = hit.point;
            var deltaMovement = currentWorldPosition - lastMouseWorldPosition;

            // Scale along the selected axis only
            var scaleChange = Vector3.zero;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // Uniform scaling when Shift is held
                var uniformDelta = (deltaMovement.x + deltaMovement.y + deltaMovement.z) / 3f * scaleFactor;
                scaleChange = new Vector3(uniformDelta, uniformDelta, uniformDelta);
            }
            else
            {
                // Project the delta movement onto the local axis direction
                var projectedDelta = Vector3.Project(deltaMovement, axisDirection);
                var scaleDelta = projectedDelta.magnitude * Mathf.Sign(Vector3.Dot(deltaMovement, axisDirection)) * scaleFactor;

                if (ActiveAxis == XAxis)
                {
                    scaleChange = new Vector3(scaleDelta, 0, 0);
                }
                else if (ActiveAxis == YAxis)
                {
                    scaleChange = new Vector3(0, scaleDelta, 0);
                }
                else if (ActiveAxis == ZAxis)
                {
                    scaleChange = new Vector3(0, 0, scaleDelta);
                }
            }

            // Apply the scale change to the object
            SelectedObject.localScale += scaleChange;

            // Ensure scale doesn't go negative or too small
            var clampedScale = SelectedObject.localScale;
            clampedScale.x = Mathf.Max(0.01f, clampedScale.x);
            clampedScale.y = Mathf.Max(0.01f, clampedScale.y);
            clampedScale.z = Mathf.Max(0.01f, clampedScale.z);
            SelectedObject.localScale = clampedScale;

            // Update the last mouse position for next frame
            lastMouseWorldPosition = currentWorldPosition;
        }
    }
}
