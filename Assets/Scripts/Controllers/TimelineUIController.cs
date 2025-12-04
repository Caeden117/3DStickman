using TMPro;
using UnityEngine;

namespace Stickman3D
{
    public class TimelineUIController : MonoBehaviour
    {
        [SerializeField] private TimelineController timelineController;

        [Space, SerializeField] private TextMeshProUGUI currentTimeText;
        [SerializeField] private TextMeshProUGUI currentFrameText;


        private void LateUpdate()
        {
            currentTimeText.text = $"Time: {timelineController.CurrentSeconds:0.00}s";
            currentFrameText.text = $"Frame: {timelineController.CurrentFrame}";
        }
    }
}
