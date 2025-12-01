using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Stickman3D
{
    public class TimelineEditorControllerKeyframe : MonoBehaviour
    {
        [SerializeField]
        private RectTransform rectTransform = null;

        public RectTransform GetRectTransform()
        {
            return rectTransform;
        }

        [SerializeField]
        private Button button = null;

        public Button GetButton()
        {
            return button;
        }

        private Keyframe keyframe = new Keyframe();

        public Keyframe Keyframe
        {
            get
            {
                return keyframe;
            }
            set
            {
                keyframe = value;
            }
        }
    }
}
