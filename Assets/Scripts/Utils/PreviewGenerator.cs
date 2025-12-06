using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Stickman3D
{
    public class PreviewGenerator : MonoBehaviour
    {
        [SerializeField] private Camera renderingCamera;
        [SerializeField] private float objectPadding = 1f;

        public async UniTask GeneratePreview(GameObject gameObjectPrefab, RenderTexture outputTexture)
        {
            // Make an instance of the prefab for rendering
            var instantiate = Instantiate(gameObjectPrefab, transform, false);

            // Set up camera billboard components to face the rendering camera instead of the main camera
            var cameraBillboard = instantiate.GetComponentInChildren<CameraBillboard>();
            if (cameraBillboard != null)
            {
                cameraBillboard.CameraTransform = renderingCamera.transform;
            }

            // Wait a frame to ensure all transforms and components are initialized
            await UniTask.Yield();

            // Focus the camera on the instantiated object and render to the output texture
            FocusCameraOn(instantiate);
            renderingCamera.targetTexture = outputTexture;
            renderingCamera.Render();
            renderingCamera.targetTexture = null;

            // Wait another frame to ensure rendering is complete
            await UniTask.Yield();

            // Clean up the instantiated object
            DestroyImmediate(instantiate);
        }

        private void FocusCameraOn(GameObject gameObject)
        {
            var objectBounds = new Bounds(transform.position, Vector3.zero);

            // Calculate the combined bounds of all renderers in the object, used for positioning the camera
            var allRenderers = gameObject.GetComponentsInChildren<Renderer>(); 
            if (allRenderers != null && allRenderers.Length > 0)
            {
                foreach (var renderer in allRenderers)
                {
                    objectBounds.Encapsulate(renderer.bounds);
                }
            }

            var objectCenter = objectBounds.center;
            var objectSize = objectBounds.extents.magnitude;

            // Calculate the distance the camera needs to be to fit the object in view (with some padding)
            var distance = objectSize * objectPadding / Mathf.Tan(Mathf.Deg2Rad * renderingCamera.fieldOfView / 2f);

            // Position the camera to look at the center of the object from the calculated distance
            var cameraPosition = objectCenter - (distance * renderingCamera.transform.forward);
            renderingCamera.transform.position = cameraPosition;
        }
    }
}
