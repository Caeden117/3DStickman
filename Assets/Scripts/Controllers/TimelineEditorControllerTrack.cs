using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Stickman3D
{
    public class TimelineEditorControllerTrack : MonoBehaviour
    {
        public UnityEvent<Keyframe> OnKeyframeClick = new UnityEvent<Keyframe>();
        
        [SerializeField]
        private GameObject keyframePrefab = null;

        [SerializeField]
        private Text textLabel = null;

        [SerializeField]
        private RectTransform keyframeRoot = null;

        [SerializeField]
        private LayoutElement labelLayoutElement = null;

        private List<Keyframe> keyframes = new List<Keyframe>();

        public List<Keyframe> Keyframes
        {
            get
            {
                return keyframes;
            }
            set
            {
                keyframes = value;
            }
        }

        private List<TimelineEditorControllerKeyframe> keyframeObjects = new List<TimelineEditorControllerKeyframe>();

        public List<TimelineEditorControllerKeyframe> GetKeyframeObjects()
        {
            return keyframeObjects;
        }

        private string path = "";

        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
                if (textLabel != null)
                {
                    textLabel.text = path;
                }

            }
        }

        private float pathLabelWidth = 256.0f;

        public float PathLabelWidth
        {
            get
            {
                return pathLabelWidth;
            }
            set
            {
                pathLabelWidth = value;
                if (labelLayoutElement != null)
                {
                    labelLayoutElement.minWidth = pathLabelWidth;
                }
            }
        }

        private void Start()
        {
            if (textLabel != null)
            {
                textLabel.text = path;
            }
            if (labelLayoutElement != null)
            {
                labelLayoutElement.minWidth = pathLabelWidth;
            }
        }

        private void Update()
        {
            while (keyframeObjects.Count > keyframes.Count)
            {
                Destroy(keyframeObjects[0]);
                keyframeObjects.RemoveAt(0);
            }

            while (keyframeObjects.Count < keyframes.Count)
            {
                var keyframeInstance = Instantiate(keyframePrefab, keyframeRoot);
                var keyframeObject = keyframeInstance.GetComponent<TimelineEditorControllerKeyframe>();
                keyframeObject.GetButton().onClick.AddListener(delegate
                {
                    OnKeyframeClick.Invoke(keyframeObject.Keyframe);
                });
                keyframeObjects.Add(keyframeObject);
            }

            for (var index = 0; index < keyframes.Count; ++index)
            {
                keyframeObjects[index].Keyframe = keyframes[index];
            }
        }
    }
}
