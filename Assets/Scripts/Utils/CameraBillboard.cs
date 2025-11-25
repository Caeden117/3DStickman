using UnityEngine;

namespace Stickman3D
{
    public class CameraBillboard : MonoBehaviour
    {
        private static Transform mainCameraTransform;

        private void Start()
        {
            if (mainCameraTransform == null)
            {
                mainCameraTransform = Camera.main.transform;
            }
        }

        private void LateUpdate()
        {
            if (mainCameraTransform != null)
            {
                transform.up = mainCameraTransform.up;
                transform.forward = mainCameraTransform.forward;
            }
        }
    }
}
