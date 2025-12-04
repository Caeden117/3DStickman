using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Stickman3D
{
    /// <summary>
    /// Core controller for the timeline editor.
    /// </summary>
    public class TimelineEditorController : MonoBehaviour
    {
        [Header("UI")]

        [SerializeField]
        private GraphicRaycaster graphicRaycaster = null;

        [SerializeField]
        private EventSystem eventSystem = null;

        [SerializeField]
        private GameObject tickPrefab = null;

        [SerializeField]
        private RectTransform tickRoot = null;

        [SerializeField]
        private GameObject trackPrefab = null;

        [SerializeField]
        private RectTransform trackRoot = null;

        [SerializeField]
        private RectTransform cursor = null;

        [SerializeField]
        private LayoutElement trackLabelSpacer = null;

        [SerializeField]
        private RectTransform scrollArea = null;

        [SerializeField]
        private Button buttonPlay = null;

        [SerializeField]
        private Button buttonPause = null;

        [SerializeField]
        private InputField inputFieldLength = null;

        [SerializeField]
        private KeyframeEditorController keyframeEditorController = null;

        [Header("")]

        [SerializeField]
        private TimelineController timelineController;

        [SerializeField]
        private HistoryController historyController;

        /// <summary>
        /// The pixel resolution per second at the default zoom (1.0f).
        /// </summary>
        public const float DefaultZoomResolution = 128.0f;

        /// <summary>
        /// The width of of the track path label in pixels.
        /// Necessary for calculating the keyframe area width.
        /// </summary>
        [SerializeField]
        private float trackPathLabelWidth = 256.0f;

        public float TrackPathLabelWidth
        {
            get
            {
                return trackPathLabelWidth;
            }
            set
            {
                trackPathLabelWidth = Mathf.Max(value, 32.0f);
            }
        }

        /// <summary>
        /// Timeline display offset (the left-most timestamp) in seconds.
        /// </summary>
        [SerializeField]
        private float timeOffsetStart = 0.0f;

        public float TimeOffsetStart
        {
            get
            {
                return timeOffsetStart;
            }
            set
            {
                timeOffsetStart = Mathf.Max(value, 0.0f);
            }
        }

        private Animation cachedAnimation;

        private void Start()
        {
            buttonPlay.onClick.AddListener(delegate {
                timelineController.IsPlaying = true;
            });
            buttonPause.onClick.AddListener(delegate {
                timelineController.IsPlaying = false;
            });
            inputFieldLength.onSubmit.AddListener(delegate {
                if (float.TryParse(inputFieldLength.text, out var length))
                {
                    timelineController.LoadedAnimation.Length = length;
                }
            });
            keyframeEditorController.OnKeyframeChanged.AddListener(delegate (string path, Keyframe oldKeyframe, Keyframe newKeyframe)
            {
                historyController.ExecuteCommand(new KeyframeEditCommand(timelineController.LoadedAnimation, keyframeEditorController, path, oldKeyframe, newKeyframe));
            });

            // rejrfwrfegserhnrjebng HACK remember to set this when loaded animation length changes (NEED TO REWRITE TO USE EVENTS)
            inputFieldLength.SetTextWithoutNotify(timelineController.LoadedAnimation.Length.ToString());
        }

        private void Update()
        {
            // Should refresh on changes instead, BUT this works for now!

            Refresh();

        }

        private bool IsMouseHover(RectTransform element, bool raycast)
        {
            if (!raycast)
            {
                Vector2 localMousePosition = element.InverseTransformPoint(Input.mousePosition);
                return element.rect.Contains(localMousePosition);
            }

            var pointerEventData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            var raycastResults = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerEventData, raycastResults);
            foreach (var raycastResult in raycastResults)
            {
                if (raycastResult.gameObject == element.gameObject)
                {
                    return true;
                }
            }
            return false;
        }

        private List<TimelineEditorControllerTick> tickObjects = new List<TimelineEditorControllerTick>();

        private Dictionary<string, TimelineEditorControllerTrack> trackObjects = new Dictionary<string, TimelineEditorControllerTrack>();

        // path to keyframe button to keyframe
        // temporary, hacky, i know i know
        private Dictionary<string, Dictionary<Keyframe, Button>> keyframeObjects = new Dictionary<string, Dictionary<Keyframe, Button>>();

        private void Refresh()
        {
            // Refresh ticks.
            // TODO: Proper zoom resolution.

            trackLabelSpacer.minWidth = trackPathLabelWidth;

            var timelineWidth = Mathf.Max(trackRoot.rect.width - trackPathLabelWidth, 0.0f);
            var timeOffsetEnd = timeOffsetStart + (timelineWidth / DefaultZoomResolution);// In seconds.

            // Time in seconds between each tick.
            var tickInterval = 1.0f;// Temporary. Will eventually use zoom resolution to calculate.

            // Number of ticks to draw between start and end offsets.
            var tickCount = Mathf.CeilToInt((timeOffsetEnd - timeOffsetStart) / tickInterval);

            while (tickObjects.Count > tickCount)
            {
                Destroy(tickObjects[0].gameObject);
                tickObjects.RemoveAt(0);
            }

            while (tickObjects.Count < tickCount)
            {
                var tickInstance = Instantiate(tickPrefab, tickRoot);
                var tickObject = tickInstance.GetComponent<TimelineEditorControllerTick>();
                tickObjects.Add(tickObject);
            }

            // The first tick timestamp.
            var tickStart = Mathf.Ceil(timeOffsetStart / tickInterval) * tickInterval;
            var tickIndex = 0;
            foreach (var tickObject in tickObjects)
            {
                var tickTimestamp = tickStart + ((float)tickIndex * tickInterval);
                var tickPosition = Mathf.InverseLerp(timeOffsetStart, timeOffsetEnd, tickTimestamp) * timelineWidth;
                tickObject.Timestamp = tickTimestamp;
                tickObject.TryGetComponent<RectTransform>(out var rectTransform);
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(tickPosition, 0.0f);
                }
                tickIndex += 1;
            }

            // Refresh cursor.

            var timeCurrent = timelineController.CurrentSeconds;
            cursor.gameObject.SetActive((timeCurrent >= timeOffsetStart) && (timeCurrent <= timeOffsetEnd));
            // map timeCurrent between timeOffsetStart and timeOffsetEnd to trackPathLabelWidth totrackPathLabelWidth + timelineWidth
            var cursorPosition = trackPathLabelWidth + ((timeCurrent - timeOffsetStart) * timelineWidth / (timeOffsetEnd - timeOffsetStart));
            cursor.anchoredPosition = new Vector2(cursorPosition, 0.0f);

            // Refresh tracks.

            var keyframeMap = timelineController.LoadedAnimation.KeyframeMap;
            var hardRefresh = cachedAnimation != null && cachedAnimation != timelineController.LoadedAnimation;
            cachedAnimation = timelineController.LoadedAnimation;

            // Remove deleted tracks.
            foreach (var path in keyframeMap.Keys)
            {
                if ((!keyframeMap.ContainsKey(path) || hardRefresh) && trackObjects.ContainsKey(path))
                {
                    DestroyImmediate(trackObjects[path].gameObject);
                    trackObjects.Remove(path);
                }
            }

            // Add new track objects.
            foreach (var path in keyframeMap.Keys)
            {
                if (!trackObjects.ContainsKey(path) || hardRefresh)
                {
                    var trackInstance = Instantiate(trackPrefab, trackRoot);
                    var trackObject = trackInstance.GetComponent<TimelineEditorControllerTrack>();
                    trackObject.Path = path;
                    trackObject.Keyframes = keyframeMap[path];
                    trackObject.OnKeyframeClick.AddListener(delegate (Keyframe keyframe)
                    {
                        keyframeEditorController.SetCurrent(path, keyframe);
                    });
                    trackObjects.Add(path, trackObject);
                }

            }

            // Resize track path labels.
            foreach (var trackObject in trackObjects.Values)
            {
                trackObject.PathLabelWidth = trackPathLabelWidth;
            }

            // Refresh keyframes.
            foreach (var trackObject in trackObjects.Values)
            {
                foreach (var keyframeObject in trackObject.GetKeyframeObjects())
                {
                    var keyframe = keyframeObject.Keyframe;
                    keyframeObject.gameObject.SetActive((keyframe.Time >= timeOffsetStart) && (keyframe.Time <= timeOffsetEnd));
                    var keyframeObjectPosition = (keyframe.Time - timeOffsetStart) * timelineWidth / (timeOffsetEnd - timeOffsetStart);
                    keyframeObject.GetRectTransform().anchoredPosition = new Vector2(keyframeObjectPosition, 0.0f);
                }
            }

            // Process input.
            if (Input.GetMouseButton(0) && IsMouseHover(tickRoot, true))
            {
                var mousePositionX = Input.mousePosition.x;
                var corners = new Vector3[4];
                tickRoot.GetWorldCorners(corners);
                var positionMin = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
                var positionMax = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
                // remap mousePosition.x between positionMin and positionMax to timeOffsetStart and timeOffsetEnd
                var pos = timeOffsetStart + ((mousePositionX - positionMin) * (timeOffsetEnd - timeOffsetStart) / (positionMax - positionMin));
                timelineController.CurrentSeconds = pos;

                // snap to nearest keyframe
                timelineController.CurrentFrame = timelineController.CurrentFrame;
            }

            if (Input.GetKey(KeyCode.LeftShift) && (Input.mouseScrollDelta.y != 0.0f) && IsMouseHover(scrollArea, false))
            {
                TimeOffsetStart -= Input.mouseScrollDelta.y * 32.0f / DefaultZoomResolution;
            }

        }
    }
}
