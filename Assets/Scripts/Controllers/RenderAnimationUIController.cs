using TMPro;
using UnityEngine;

namespace Stickman3D
{
    public class RenderAnimationUIController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI renderStatusText;
        [SerializeField] private TimelineController timelineController;

        private string renderTextTemplate;
        private int frameLength;

        private void OnEnable()
        {
            renderTextTemplate ??= renderStatusText.text;

            frameLength = Mathf.CeilToInt(timelineController.LoadedAnimation.Length * timelineController.Framerate);
        }

        private void LateUpdate()
            => renderStatusText.text = string.Format(
                renderTextTemplate,
                timelineController.CurrentFrame,
                frameLength
            );
    }
}
