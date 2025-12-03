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

        // Root SceneNode prefab to hold initial animation state, exposed in inspector
        [SerializeField]
        private SceneNode rootNodePrefab;
        private SceneNode rootNode;

        // SceneNode path cache
        private readonly Dictionary<string, SceneNode> sceneNodeMap = new();

        /// <summary>
        /// Loads a new Animation, clearing the root node and instantiating objects as per the ObjectMap.
        /// </summary>
        public void LoadAnimation(Animation animation)
        {
            LoadedAnimation = animation;

            // Clear cache and regenerate root node
            sceneNodeMap.Clear();
            
            if (rootNode != null)
            {
                DestroyImmediate(rootNode.gameObject);
            }
            rootNode = Instantiate(rootNodePrefab, Vector3.zero, Quaternion.identity);

            if (LoadedAnimation.ObjectMap == null) return;

            // Instantiate objects from ObjectMap
            // Iterating over a Dictionary isn't the best but whatever
            foreach (var kvp in LoadedAnimation.ObjectMap)
            {
                var objectName = kvp.Key;
                var resourcePath = kvp.Value;

                // Instantiate object without adding to ObjectMap (already present)
                CreateObject(objectName, resourcePath, false);
            }
        }

        /// <summary>
        /// Instantiates a prefab from Resources at <paramref name="resourcePath"/> under the root node with the given <paramref name="objectName"/>.
        /// Optionally adds the object to the LoadedAnimation's ObjectMap.
        /// </summary>
        public void CreateObject(string objectName, string resourcePath, bool addToObjectMap = true)
        {
            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                Debug.LogWarning($"Failed to load prefab at Resource path '{resourcePath}' for SceneNode '{objectName}'.");
                return;
            }

            // Instantiate prefab under root node
            var instance = Instantiate(prefab, rootNode.transform);
            instance.name = objectName;

            if (addToObjectMap)
            {
                LoadedAnimation.ObjectMap[objectName] = resourcePath;
            }
        }

        /// <summary>
        /// Returns the list of Keyframes for the given SceneNode path.
        /// Because the list is passed by reference, modifications to the list will be reflected in the animation.
        /// Returns null if no animation is loaded or if <paramref name="scenePath"/> is not in the KeyframeMap.
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
        /// Gets the path of the given SceneNode relative to the root node, or null if none exists.
        /// </summary>
        public string GetPathForNode(SceneNode node) => node.GetScenePathRelativeTo(rootNode);

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

            // Iterating over a Dictionary isn't the best but whatever
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
                int rightKeyframeIdx;
                for (rightKeyframeIdx = 0; rightKeyframeIdx < keyframes.Count; rightKeyframeIdx++)
                {
                    if (keyframes[rightKeyframeIdx].Time > time)
                    {
                        break;
                    }
                }
                var leftKeyframeIdx = rightKeyframeIdx - 1;

                var leftKeyframe = keyframes[Mathf.Clamp(leftKeyframeIdx, 0, keyframes.Count - 1)];
                var rightKeyframe = keyframes[Mathf.Clamp(rightKeyframeIdx, 0, keyframes.Count - 1)];

                // Interpolate between the two keyframes if necessary
                var matrix = leftKeyframe.Transform;
                if (leftKeyframeIdx != rightKeyframeIdx && !MatrixUtils.IsApproximatelyEqual(leftKeyframe.Transform, rightKeyframe.Transform))
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
                currentTime = Mathf.Repeat(currentTime + Time.deltaTime, LoadedAnimation.Length);
            }
            InterpolateAnimation(currentTime);
        }
    }
}
