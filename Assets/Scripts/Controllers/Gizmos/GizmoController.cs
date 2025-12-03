using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Stickman3D.Gizmos
{
    /// <summary>
    /// Abstract base class for individual gizmos (position, rotation, scale)
    /// </summary>
    public abstract class GizmoController : MonoBehaviour
    {
        [Header("Axis Transforms")]
        [SerializeField] protected Transform XAxis;
        [SerializeField] protected Transform YAxis;
        [SerializeField] protected Transform ZAxis;
        
        [Header("Extended Axis Transforms")]
        [SerializeField] private Transform extendedXAxis;
        [SerializeField] private Transform extendedYAxis;
        [SerializeField] private Transform extendedZAxis;

        [Header("References")]
        [SerializeField] private GizmoManager gizmoManager;
        [SerializeField] private TimelineController timelineController;
        [SerializeField] private HistoryController historyController;
        [SerializeField] private KeyframeEditorController keyframeEditorController;

        // Convenience accessor properties from the GizmoManager
        protected Transform SelectedObject => gizmoManager != null ? gizmoManager.SelectedObject.transform : null;
        protected bool IsMouseDown => gizmoManager.IsMouseDown;
        protected Ray MouseRay => gizmoManager.MouseRay;
        protected LayerMask GizmoLayer => gizmoManager.GizmoLayer;

        // Interaction state
        protected bool IsInteracting = false;
        protected Transform ActiveAxis = null;

        // Private keyframe management
        private Animation loadedAnimation;
        private string objectPath;
        private Keyframe workingKeyframe;
        private Keyframe? existingKeyframe;

        private void Start() => DisableExtendedAxes();

        /// <summary>
        /// Called by GizmoManager to handle input for this gizmo type
        /// </summary>
        public void HandleInput()
        {
            if (SelectedObject == null)
                return;

            if (IsMouseDown && !IsInteracting)
            {
                TryStartInteraction();
            }
            else if (IsMouseDown && IsInteracting)
            {
                UpdateInteraction();
            }
            else if (!IsMouseDown && IsInteracting)
            {
                EndInteraction();
            }
        }

        /// <summary>
        /// Called when the gizmo should be activated
        /// </summary>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
            
            if (!active && IsInteracting)
            {
                EndInteraction();
            }
        }

        // Attempt to start an interaction with this gizmo
        private void TryStartInteraction()
        {
            // Raycast against gizmo elements
            if (Physics.Raycast(MouseRay, out var hit, Mathf.Infinity, GizmoLayer))
            {
                // Attempt to get the base axis that was hit
                var baseAxis = GetBaseAxisFromElement(hit.transform);
                if (baseAxis != null)
                {
                    StartInteraction(baseAxis, hit.point);
                }
            }
        }

        // Start an interaction with the specified gizmo element
        private void StartInteraction(Transform baseAxis, Vector3 hitPoint)
        {
            // Initialize interaction state
            IsInteracting = true;
            ActiveAxis = baseAxis;

            // Prepare working keyframe
            loadedAnimation = timelineController.LoadedAnimation;
            objectPath = timelineController.GetPathForNode(gizmoManager.SelectedObject);
            workingKeyframe = new Keyframe
            {
                Time = timelineController.CurrentSeconds,
                Transform = SelectedObject.GetLocalMatrix()
            };

            // Check for existing keyframe at current time
            existingKeyframe = loadedAnimation.FindKeyframeAtTime(objectPath, workingKeyframe.Time);

            // Enable extended axis and disable others based on which axis was clicked
            EnableExtendedAxisForInteraction(baseAxis);

            OnInteractionStart(baseAxis, hitPoint);
        }

        // Update the ongoing interaction
        private void UpdateInteraction()
        {
            if (ActiveAxis == null)
                return;

            // Call the specific gizmo's update logic
            OnInteractionUpdate();

            // Remove working keyframe from animation
            loadedAnimation.RemoveKeyframe(objectPath, workingKeyframe);

            // Update working keyframe with new transform
            workingKeyframe.Time = timelineController.CurrentSeconds;
            workingKeyframe.Transform = SelectedObject.GetLocalMatrix();

            // Add updated working keyframe back to animation
            loadedAnimation.InsertKeyframe(objectPath, workingKeyframe);
        }

        // End the current interaction
        private void EndInteraction()
        {
            if (!IsInteracting)
                return;

            // Finalize keyframe as a new command
            loadedAnimation.RemoveKeyframe(objectPath, workingKeyframe);

            ICommand command = existingKeyframe == null
                ? new KeyframeAddCommand(loadedAnimation, keyframeEditorController, objectPath, workingKeyframe)
                : new KeyframeEditCommand(loadedAnimation, keyframeEditorController, objectPath, existingKeyframe.Value, workingKeyframe);

            historyController.ExecuteCommand(command);

            // Reset axis visibility
            DisableExtendedAxes();
            EnableAllAxes();

            // Reset interaction state
            IsInteracting = false;
            ActiveAxis = null;
        }

        // Enable the extended axis for the clicked axis and disable other axes
        private void EnableExtendedAxisForInteraction(Transform baseAxis)
        {
            if (baseAxis == XAxis)
            {
                extendedXAxis.gameObject.SetActive(true);
                YAxis.gameObject.SetActive(false);
                ZAxis.gameObject.SetActive(false);
            }
            else if (baseAxis == YAxis)
            {
                extendedYAxis.gameObject.SetActive(true);
                XAxis.gameObject.SetActive(false);
                ZAxis.gameObject.SetActive(false);
            }
            else if (baseAxis == ZAxis)
            {
                extendedZAxis.gameObject.SetActive(true);
                XAxis.gameObject.SetActive(false);
                YAxis.gameObject.SetActive(false);
            }
        }

        // Get the base axis transform from any gizmo element (handles children and extended axes)
        [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Code is cleaner this way")]
        private Transform GetBaseAxisFromElement(Transform element)
        {
            // Check if it's a direct axis
            if (element == XAxis || element == extendedXAxis) return XAxis;
            if (element == YAxis || element == extendedYAxis) return YAxis;
            if (element == ZAxis || element == extendedZAxis) return ZAxis;

            // Check if it's a child of an axis
            if (element.parent == XAxis) return XAxis;
            if (element.parent == YAxis) return YAxis;
            if (element.parent == ZAxis) return ZAxis;

            return null;
        }

        // Disable all extended axes
        private void DisableExtendedAxes()
        {
            if (extendedXAxis != null) extendedXAxis.gameObject.SetActive(false);
            if (extendedYAxis != null) extendedYAxis.gameObject.SetActive(false);
            if (extendedZAxis != null) extendedZAxis.gameObject.SetActive(false);
        }

        // Enable all main axes
        private void EnableAllAxes()
        {
            if (XAxis != null) XAxis.gameObject.SetActive(true);
            if (YAxis != null) YAxis.gameObject.SetActive(true);
            if (ZAxis != null) ZAxis.gameObject.SetActive(true);
        }

        // Abstract methods to be implemented by specific gizmo types
        
        /// <summary>
        /// Called when an interaction starts
        /// </summary>
        protected abstract void OnInteractionStart(Transform baseAxis, Vector3 hitPoint);

        /// <summary>
        /// Called each frame during an interaction
        /// </summary>
        protected abstract void OnInteractionUpdate();
    }
}
