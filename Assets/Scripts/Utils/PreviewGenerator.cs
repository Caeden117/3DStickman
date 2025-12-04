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
            var instantiate = Instantiate(gameObjectPrefab, transform, false);

            // Wait a frame to ensure all transforms and components are initialized
            await UniTask.Yield();

            FocusCameraOn(instantiate);

            renderingCamera.targetTexture = outputTexture;
            renderingCamera.Render();
            renderingCamera.targetTexture = null;

            // Wait another frame to ensure rendering is complete
            await UniTask.Yield();

            DestroyImmediate(instantiate);
        }

        private void FocusCameraOn(GameObject gameObject)
        {
            var objectBounds = default(Bounds);

            var allRenderers = gameObject.GetComponentsInChildren<Renderer>(); 
            if (allRenderers != null && allRenderers.Length > 0)
            {
                foreach (var renderer in allRenderers)
                {
                    objectBounds.Encapsulate(renderer.localBounds);
                }
            }

            var objectCenter = objectBounds.center;
            var objectSize = objectBounds.extents.magnitude;

            var distance = objectSize * objectPadding / Mathf.Tan(Mathf.Deg2Rad * renderingCamera.fieldOfView / 2f);

            var cameraPosition = objectCenter - (distance * renderingCamera.transform.forward);

            renderingCamera.transform.localPosition = cameraPosition;
        }
    }
}
