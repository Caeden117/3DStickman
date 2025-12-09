using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Stickman3D
{
    public class TimelineEditorControllerResize : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private TimelineEditorController timelineEditorController = null;

        [SerializeField]
        private RectTransform viewport = null;

        private bool pressed = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            pressed = true;
        }

        private void Update()
        {
            if (pressed)
            {
                var mousePositionX = Input.mousePosition.x;
                var corners = new Vector3[4];
                viewport.GetWorldCorners(corners);
                var positionMin = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);

                timelineEditorController.TrackPathLabelWidth = (mousePositionX - positionMin) / viewport.lossyScale.x;

                if (!Input.GetMouseButton(0))
                {
                    pressed = false;
                }
            }
        }
    }
}
