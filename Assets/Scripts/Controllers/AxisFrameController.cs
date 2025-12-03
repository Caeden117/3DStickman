using UnityEngine;

namespace Stickman3D
{
    public class AxisFrameController : MonoBehaviour
    {
        public Transform SelectedObject { get; private set; }

        [SerializeField]
        private Transform xAxis;

        [SerializeField]
        private Transform extendedXAxis;

        [SerializeField]
        private Transform yAxis;

        [SerializeField]
        private Transform extendedYAxis;

        [SerializeField]
        private Transform zAxis;

        [SerializeField]
        private Transform extendedZAxis;

        [SerializeField]
        private LayerMask axisLayer;
        private Vector3? lastMousePosition;

        public void SelectObject(Transform obj)
        {
            SelectedObject = obj;

            if (obj == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            transform.position = SelectedObject.position;
        }

        private void Update()
        {
            // Early return if no mouse button is held or no object is selected
            if (!Input.GetMouseButton(0) || SelectedObject == null)
            {
                DisableExtendedAxis();
                lastMousePosition = null;
                return;
            }

            // Raycast to see which axis we are interacting with
            // Early return if we are not hitting any axis
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, axisLayer))
            {
                DisableExtendedAxis();
                lastMousePosition = null;
                return;
            }

            // Cache the starting mouse position on first click - used for calculating deltas
            if (lastMousePosition == null)
            {
                if (hitInfo.transform == xAxis)
                {
                    extendedXAxis.gameObject.SetActive(true);
                    yAxis.gameObject.SetActive(false);
                    zAxis.gameObject.SetActive(false);
                }
                else if (hitInfo.transform == yAxis)
                {
                    extendedYAxis.gameObject.SetActive(true);
                    xAxis.gameObject.SetActive(false);
                    zAxis.gameObject.SetActive(false);
                }
                else if (hitInfo.transform == zAxis)
                {
                    extendedZAxis.gameObject.SetActive(true);
                    xAxis.gameObject.SetActive(false);
                    yAxis.gameObject.SetActive(false);
                }

                lastMousePosition = hitInfo.point;
                return;
            }

            // Move the selected object along the selected axis based on mouse movement delta
            if (hitInfo.transform == xAxis || hitInfo.transform.parent == xAxis)
            {
                SelectedObject.position += new Vector3(hitInfo.point.x - lastMousePosition.Value.x, 0, 0);
            }
            else if (hitInfo.transform == yAxis || hitInfo.transform.parent == yAxis)
            {
                SelectedObject.position += new Vector3(0, hitInfo.point.y - lastMousePosition.Value.y, 0);
            }
            else if (hitInfo.transform == zAxis || hitInfo.transform.parent == zAxis)
            {
                SelectedObject.position += new Vector3(0, 0, hitInfo.point.z - lastMousePosition.Value.z);
            }

            lastMousePosition = hitInfo.point;
            transform.position = SelectedObject.position;
        }

        private void DisableExtendedAxis()
        {
            extendedXAxis.gameObject.SetActive(false);
            extendedYAxis.gameObject.SetActive(false);
            extendedZAxis.gameObject.SetActive(false);

            xAxis.gameObject.SetActive(true);
            yAxis.gameObject.SetActive(true);
            zAxis.gameObject.SetActive(true);
        }
    }

}
