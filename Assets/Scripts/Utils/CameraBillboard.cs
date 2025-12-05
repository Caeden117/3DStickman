using UnityEngine;

namespace Stickman3D
{
    public class CameraBillboard : MonoBehaviour
    {
        public Transform CameraTransform;

        private void Start()
        {
            if (CameraTransform == null)
            {
                CameraTransform = Camera.main.transform;
            }
        }

        private void LateUpdate()
        {
            if (CameraTransform != null)
            {
                transform.up = CameraTransform.up;
                transform.forward = CameraTransform.forward;
            }
        }
    }
}
