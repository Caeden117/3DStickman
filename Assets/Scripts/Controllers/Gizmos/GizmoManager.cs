using UnityEngine;

namespace Stickman3D.Gizmos
{
    public enum GizmoMode
    {
        Position,
        Rotation,
        Scale
    }

    /// <summary>
    /// Gizmo manager that handles switching between different gizmo modes and gizmo interactions.
    /// </summary>
    public class GizmoManager : MonoBehaviour
    {
        [Header("Gizmo Controllers")]
        [SerializeField] private PositionGizmoController positionGizmo;
        [SerializeField] private RotationGizmoController rotationGizmo;
        [SerializeField] private ScaleGizmoController scaleGizmo;

        [Header("Settings")]
        [SerializeField] private GizmoMode currentMode = GizmoMode.Position;
        [SerializeField] private LayerMask gizmoLayer;
        [SerializeField] private LayerMask sceneNodeLayer;

        [Header("Gizmo Keybinds")]
        [SerializeField] private KeyCode positionModeKey = KeyCode.G;
        [SerializeField] private KeyCode rotationModeKey = KeyCode.R;
        [SerializeField] private KeyCode scaleModeKey = KeyCode.S;

        [Header("References")]
        [SerializeField] private HistoryController historyController;
        [SerializeField] private TimelineController timelineController;

        [Header("Display Settings")]
        [SerializeField] private float baseGizmoScale = 0.1f;
        [SerializeField] private float screenSizeMultiplier = 0.02f;

        public SceneNode SelectedObject { get; private set; }
        public GizmoMode CurrentMode 
        { 
            get => currentMode; 
            set 
            { 
                if (currentMode != value)
                {
                    currentMode = value;
                    UpdateGizmoVisibility();
                }
            } 
        }

        // Shared input state
        public bool IsMouseDown { get; private set; }
        public bool IsMouseDragging { get; private set; }
        public Vector3 MousePosition => Input.mousePosition;
        public Ray MouseRay => Camera.main.ScreenPointToRay(MousePosition);
        public LayerMask GizmoLayer => gizmoLayer;

        private GizmoController currentActiveGizmo;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
            UpdateGizmoVisibility();
        }

        private void Update()
        {
            HandleGizmoInput();
            UpdateInputState();
            HandleObjectDeletion();
            HandleObjectSelection();

            if (SelectedObject == null)
                return;

            // Update gizmo position to match selected object
            SelectedObject.transform.GetPositionAndRotation(out var pos, out var rot);
            transform.SetPositionAndRotation(pos, rot);

            // Update gizmo scale based on distance to camera
            UpdateGizmoScale(pos);

            // Handle input through the active gizmo
            if (currentActiveGizmo != null)
            {
                currentActiveGizmo.HandleInput();
            }
        }

        // Update gizmo mode based on key input
        private void HandleGizmoInput()
        {
            if (Input.GetKeyDown(positionModeKey))
            {
                CurrentMode = GizmoMode.Position;
            }
            else if (Input.GetKeyDown(rotationModeKey))
            {
                CurrentMode = GizmoMode.Rotation;
            }
            else if (Input.GetKeyDown(scaleModeKey))
            {
                CurrentMode = GizmoMode.Scale;
            }
        }

        // Update shared input state amongst all gizmos
        private void UpdateInputState()
        {
            // Disable gizmo interaction when Alt is held (for camera control)
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                IsMouseDown = false;
                IsMouseDragging = false;
                return;
            }

            var wasMouseDown = IsMouseDown;
            IsMouseDown = Input.GetMouseButton(0);
            
            if (!wasMouseDown && IsMouseDown)
            {
                // Mouse just pressed
                IsMouseDragging = false;
            }
            else if (IsMouseDown && !IsMouseDragging)
            {
                // For simplicity, start dragging immediately
                IsMouseDragging = true;
            }
            else if (wasMouseDown && !IsMouseDown)
            {
                // Mouse just released
                IsMouseDragging = false;
            }
        }

        // Handle deletion of the selected object
        private void HandleObjectDeletion()
        {
            if (SelectedObject == null)
                return;

            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            {
                var animation = timelineController.LoadedAnimation;

                // Sanity check
                if (!animation.ObjectMap.ContainsKey(SelectedObject.name))
                {
                    Debug.LogWarning("Tried to delete an object that does not exist in the animation.\n" +
                        "More than likely, this is an object that was part of the default SceneNode Root prefab.");
                    return;
                }

                var objectName = SelectedObject.name;

                var resourcePath = timelineController.LoadedAnimation.ObjectMap[objectName];
                var keyframes = timelineController.LoadedAnimation.KeyframeMap[objectName];

                historyController.ExecuteCommand(new ObjectDeleteCommand(timelineController, objectName, resourcePath, keyframes));

                SelectObject(null);
            }
        }

        // Handle click to select scene nodes
        private void HandleObjectSelection()
        {
            // Check for Shift + Left click for object selection
            if (!(Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift)))
                return;

            // Raycast into the world to find SceneNodes
            if (!Physics.Raycast(MouseRay, out var hit, Mathf.Infinity, sceneNodeLayer))
            {
                // No object hit, deselect current object
                SelectObject(null);
                return;
            }

            // Try to find a SceneNode component on the hit object
            if (!hit.collider.TryGetComponent<SceneNode>(out var sceneNode))
            {
                // If not found, check the parent
                // (We are not using GetComponentInParent because we only check one level up)
                hit.transform.parent.TryGetComponent(out sceneNode);
            }

            // Select the SceneNode (or null if not found)
            SelectObject(sceneNode);
        }

        // Update gizmo scale based on distance to camera
        private void UpdateGizmoScale(Vector3 gizmoPosition)
        {
            var distance = Vector3.Distance(mainCamera.transform.position, gizmoPosition);
            var scale =  baseGizmoScale * distance * screenSizeMultiplier;
            transform.localScale = scale * Vector3.one;
        }

        /// <summary>
        /// Selects an object for manipulation with gizmos.
        /// </summary>
        public void SelectObject(SceneNode obj)
        {
            SelectedObject = obj;

            gameObject.SetActive(obj != null);

            UpdateGizmoVisibility();
        }

        // Updates gizmo visibility based on the current mode
        private void UpdateGizmoVisibility()
        {
            // Disable all gizmos first
            if (positionGizmo != null) positionGizmo.SetActive(false);
            if (rotationGizmo != null) rotationGizmo.SetActive(false);
            if (scaleGizmo != null) scaleGizmo.SetActive(false);

            // Enable the current mode gizmo
            currentActiveGizmo = null;

            if (SelectedObject != null)
            {
                switch (currentMode)
                {
                    case GizmoMode.Position:
                        if (positionGizmo != null)
                        {
                            positionGizmo.SetActive(true);
                            currentActiveGizmo = positionGizmo;
                        }
                        break;
                    case GizmoMode.Rotation:
                        if (rotationGizmo != null)
                        {
                            rotationGizmo.SetActive(true);
                            currentActiveGizmo = rotationGizmo;
                        }
                        break;
                    case GizmoMode.Scale:
                        if (scaleGizmo != null)
                        {
                            scaleGizmo.SetActive(true);
                            currentActiveGizmo = scaleGizmo;
                        }
                        break;
                }
            }
        }

        // Public methods for switching modes
        public void SetPositionMode() => CurrentMode = GizmoMode.Position;
        public void SetRotationMode() => CurrentMode = GizmoMode.Rotation;
        public void SetScaleMode() => CurrentMode = GizmoMode.Scale;
    }
}
