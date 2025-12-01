using UnityEngine;
using UnityEngine.UI;

namespace Stickman3D
{
    public class TimelineEditorControllerTick : MonoBehaviour
    {
        [SerializeField]
        private Text textLabel = null;

        private float timestamp = 0.0f;

        public float Timestamp
        {
            get
            {
                return timestamp;
            }
            set
            {
                timestamp = value;
                if (textLabel != null)
                {
                    textLabel.text = timestamp.ToString("F2");
                }

            }
        }

        private void Start()
        {
            if (textLabel != null)
            {
                textLabel.text = timestamp.ToString("F2");
            }
        }
    }
}
