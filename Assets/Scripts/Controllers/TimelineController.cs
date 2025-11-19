using System.Collections.Generic;
using UnityEngine;

namespace Stickman3D
{
    public class TimelineController : MonoBehaviour
    {
        #region Playback Management
        /// <summary>
        /// Playback state of the timeline.
        /// </summary>
        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                isPlaying = value;

                if (!isPlaying)
                {
                    CurrentFrame = CurrentFrame;
                }
            }
        }

        /// <summary>
        /// Framerate of the animation, in frames per second.
        /// This is purely editor-side.
        /// </summary>
        public int Framerate { get; set; } = 30;

        /// <summary>
        /// Current playback time, in seconds.
        /// </summary>
        public float CurrentSeconds
        {
            get => currentTime; set
            {
                currentTime = Mathf.Max(0, value);
                InterpolateAnimation(currentTime);
            }
        }

        /// <summary>
        /// Current playback time, in frames.
        /// </summary>
        public int CurrentFrame
        {
            get => Mathf.FloorToInt(currentTime * Framerate);
            set => CurrentSeconds = Mathf.Max(0, value) / (float)Framerate;
        }

        private float currentTime;
        private bool isPlaying;
        #endregion

        #region Animation Management
        public Animation LoadedAnimation { get; private set; }

        // Root SceneNode to hold animation, exposed in inspector
        [SerializeField]
        private SceneNode rootNode;

        // SceneNode path cache
        private readonly Dictionary<string, SceneNode> sceneNodeMap = new();

        /// <summary>
        /// Loads a new Animation, clearing the root node and instantiating objects as per the ObjectMap.
        /// </summary>
        public void LoadAnimation(Animation animation)
        {
            LoadedAnimation = animation;

            // Clear cache
            sceneNodeMap.Clear();

            // Instantiate objects from ObjectMap
            if (LoadedAnimation.ObjectMap == null) return;
            foreach (var kvp in LoadedAnimation.ObjectMap)
            {
                // Iterating over a Dictionary isn't the best but whatever
                var objectName = kvp.Key;
                var resourcePath = kvp.Value;

                var prefab = Resources.Load<GameObject>(resourcePath);
                if (prefab == null)
                {
                    Debug.LogWarning($"Failed to load prefab at Resource path '{resourcePath}' for SceneNode '{objectName}'.");
                    continue;
                }

                // Instantiate prefab under root node
                var instance = Instantiate(prefab, rootNode.transform);
                instance.name = objectName;
            }
        }

        /// <summary>
        /// Returns the list of Keyframes for the given SceneNode path.
        /// Because the list is passed by reference, modifications to the list will be reflected in the animation.
        /// Returns null if no animation is loaded.
        /// </summary>
        public List<Keyframe> GetKeyframesForPath(string scenePath)
        {
            if (LoadedAnimation == null || LoadedAnimation.KeyframeMap == null)
            {
                return null;
            }

            if (!LoadedAnimation.KeyframeMap.TryGetValue(scenePath, out var keyframes))
            {
                keyframes = new();
                LoadedAnimation.KeyframeMap[scenePath] = keyframes;
            }

            return keyframes;
        }

        /// <summary>
        /// Gets the SceneNode at the given path, or null if not found.
        /// </summary>
        public SceneNode GetNodeAtPath(string scenePath)
        {
            // Hot path: check cache first
            if (sceneNodeMap.TryGetValue(scenePath, out var node))
            {
                return node;
            }

            // Use Unity API to find the node's Transform, starting from the rootNode
            var sceneTransform = rootNode.transform.Find(scenePath);
            if (sceneTransform == null)
            {
                return null;
            }

            // Try get SceneNode component and cache if found, return null otherwise
            if (sceneTransform.TryGetComponent(out node))
            {
                sceneNodeMap[scenePath] = node;
            }

            return node;
        }

        // Interpolates current animation state at the given time
        private void InterpolateAnimation(float time)
        {
            if (LoadedAnimation == null || LoadedAnimation.KeyframeMap == null) return;

            foreach (var kvp in LoadedAnimation.KeyframeMap)
            {
                var scenePath = kvp.Key;
                var keyframes = kvp.Value;

                var node = GetNodeAtPath(scenePath);
                if (node == null || keyframes.Count == 0) continue;

                // Find the two keyframes to interpolate between
                // While its faster on average to do a binary search, I'm opting for a linear search here
                //   since most animations will have relatively few keyframes per object.
                // If performance becomes an issue, this can be changed to binary search later.
                int leftKeyframeIdx;
                for (leftKeyframeIdx = 0; leftKeyframeIdx < keyframes.Count; leftKeyframeIdx++)
                {
                    if (keyframes[leftKeyframeIdx].Time <= time)
                    {
                        continue;
                    }

                    break;
                }
                var rightKeyframeIdx = Mathf.Clamp(leftKeyframeIdx + 1, 0, keyframes.Count);

                var leftKeyframe = keyframes[leftKeyframeIdx];
                var rightKeyframe = keyframes[rightKeyframeIdx];

                var matrix = leftKeyframe.Transform;

                // Interpolate between the two keyframes
                if (leftKeyframeIdx != rightKeyframeIdx)
                {
                    var t = (time - leftKeyframe.Time) / (rightKeyframe.Time - leftKeyframe.Time);
                    matrix = MatrixUtils.Lerp(leftKeyframe.Transform, rightKeyframe.Transform, t);
                }

                // UNITY DOES NOT SUPPORT SETTING TRANFORM MATRIX DIRECTLY, KILLING MYSELF
                // oh well, at least the matrix in JSON will be in local space relative to parent
                var localPosition = matrix.GetColumn(3);
                var localRotation = matrix.rotation;
                var localScale = matrix.lossyScale;

                node.transform.SetLocalPositionAndRotation(localPosition, localRotation);
                node.transform.localScale = localScale;
            }
        }
        #endregion

        private void Update()
        {
            if (isPlaying)
            {
                currentTime += Time.deltaTime;
                InterpolateAnimation(currentTime);
            }
        }
    }
}
