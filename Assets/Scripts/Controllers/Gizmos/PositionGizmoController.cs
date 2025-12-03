using UnityEngine;

namespace Stickman3D.Gizmos
{
    public class PositionGizmoController : GizmoController
    {
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

            // Project the delta movement onto the local axis direction
            var projectedDelta = Vector3.Project(deltaMovement, axisDirection);

            // Apply the projected movement to the object
            SelectedObject.position += projectedDelta;

            // Update the gizmo position to follow the selected object
            transform.position = SelectedObject.position;

            // Update the last mouse position for next frame
            lastMouseWorldPosition = currentWorldPosition;
        }
    }
}
