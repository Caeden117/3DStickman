using UnityEngine;

namespace Stickman3D.Gizmos
{
    public class RotationGizmoController : GizmoController
    {
        private Quaternion initialRotation;
        private Vector3 initialHitPoint;
        private Vector3 rotationCenter;
        private Vector3 rotationAxis;

        protected override void OnInteractionStart(Transform baseAxis, Vector3 hitPoint)
        {
            // Cache the starting hit point and rotation center
            initialHitPoint = hitPoint;
            rotationCenter = SelectedObject.position;
            initialRotation = SelectedObject.rotation;

            // Determine rotation axis in local space based on which gizmo axis is active
            if (ActiveAxis == XAxis)
            {
                rotationAxis = SelectedObject.right;
            }
            else if (ActiveAxis == YAxis)
            {
                rotationAxis = SelectedObject.up;
            }
            else if (ActiveAxis == ZAxis)
            {
                rotationAxis = SelectedObject.forward;
            }
        }

        protected override void OnInteractionUpdate()
        {
            // Raycast to get current mouse world position
            if (!Physics.Raycast(MouseRay, out var hit, Mathf.Infinity, GizmoLayer))
                return;

            var currentHitPoint = hit.point;

            // Calculate vectors from rotation center to initial and current hit points
            var initialVector = (initialHitPoint - rotationCenter).normalized;
            var currentVector = (currentHitPoint - rotationCenter).normalized;

            // Project vectors onto the plane perpendicular to the rotation axis
            var projectedInitialVector = Vector3.ProjectOnPlane(initialVector, rotationAxis).normalized;
            var projectedCurrentVector = Vector3.ProjectOnPlane(currentVector, rotationAxis).normalized;

            // Calculate the angle between the projected vectors
            var angle = Vector3.SignedAngle(projectedInitialVector, projectedCurrentVector, rotationAxis);

            // Apply rotation around the specified local axis
            var deltaRotation = Quaternion.AngleAxis(angle, rotationAxis);
            SelectedObject.rotation = deltaRotation * initialRotation;
        }
    }
}
