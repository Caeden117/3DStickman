using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Stickman3D
{
    public class CreateObjectController : MonoBehaviour
    {
        private const int previewTextureSize = 256;

        [Header("Object Item Generation")]
        [SerializeField] private CreateObjectItemController itemPrefab;
        [SerializeField] private PreviewGenerator previewGenerator;
        [SerializeField] private Transform content;

        [Header("Object Creation")]
        [SerializeField] private TimelineController timelineController;

        private SceneNode[] objectsInResouces;
        private readonly List<RenderTexture> previewTextures = new();

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is a valid suppression of a Unity Analyzer warning.")]
        [SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "This is called internally by UniTask.")]
        [SuppressMessage("TypeSafety", "UNT0006:Incorrect message signature", Justification = "This is correct for UniTask integration.")]
        private async UniTask Start()
        {
            // Gather all SceneNode objects in Resources
            objectsInResouces = Resources.LoadAll<SceneNode>("");

            // Apparently the very first object generation can result in an empty texture,
            // so we yield a frame before starting the generation process
            await UniTask.Yield();

            foreach (var sceneNode in objectsInResouces)
            {
                // Create a RenderTexture for the preview
                var renderTexture = new RenderTexture(previewTextureSize, previewTextureSize, 16, RenderTextureFormat.ARGB32);
                renderTexture.Create();
                previewTextures.Add(renderTexture);

                // Generate the preview using the PreviewGenerator
                await previewGenerator.GeneratePreview(sceneNode.gameObject, renderTexture);
                
                // Instantiate the item prefab and set its information
                var itemInstance = Instantiate(itemPrefab, content);
                itemInstance.SetObjectInformation(sceneNode.name, renderTexture);
                itemInstance.Button.onClick.AddListener(() => CreateObject(sceneNode));

                // Allow a frame to pass to keep the application responsive
                await UniTask.Yield();
            }
        }

        // TODO(Caeden): Turn into ICommand for undo/redo support
        private void CreateObject(SceneNode sceneNode)
        {
            // By default, resource path is simply the name of the GameObject
            var resourcePath = sceneNode.gameObject.name;

            // Search the loaded animation's ObjectMap to see if we already have copies of this object
            var foundDuplicates = 0;
            foreach (var existingObject in timelineController.LoadedAnimation.ObjectMap.Values)
            {
                if (existingObject == resourcePath)
                {
                    foundDuplicates++;
                }
            }

            // If duplicates were found, append a number to the end of the object name
            var objectName = foundDuplicates == 0
                ? resourcePath
                : $"{resourcePath} ({foundDuplicates})";

            // Create the object in the timeline controller and assign a default keyframe list
            timelineController.CreateObject(objectName, resourcePath);
            timelineController.LoadedAnimation.KeyframeMap[objectName] = new List<Keyframe>();
        }

        private void OnDestroy()
        {
            // Clean up RenderTextures to prevent memory leaks
            foreach (var renderTexture in previewTextures)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }
            previewTextures.Clear();
        }
    }
}
