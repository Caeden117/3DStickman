using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SFB;
using UnityEngine;

namespace Stickman3D
{
    public class SaveController : MonoBehaviour
    {
        [SerializeField] private TimelineController timelineController;

        private void Update()
        {
            // Early return if Ctrl is not held down.
            if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                return;

            if (Input.GetKeyDown(KeyCode.S))
                OpenSavePanel();

            if (Input.GetKeyDown(KeyCode.O))
                OpenOpenPanel();
        }

        #region Saving
        // Invokes UniTask to save the animation asynchronously.
        // UniTask will handle the method internally so we can forget about it here.
        public void OpenSavePanel()
            => StandaloneFileBrowser.SaveFilePanelAsync("Save Animation", "", "animation.json", "json", path => SaveAnimationAsync(path).Forget());

        // Asynchronously saves the current animation to the specified path.
        private async UniTask SaveAnimationAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Save operation cancelled or invalid path.");
                return;
            }

            // We are executing on the main Unity thread by default. Switch to a thread pool so these operations don't cause lag.
            await UniTask.SwitchToThreadPool();

            var file = new FileInfo(path);
            var writeStream = file.OpenWrite();

            // Serialize loaded animation to JSON
            var animation = timelineController.LoadedAnimation;
            var animationJson = JsonConvert.SerializeObject(animation, Formatting.None, new Matrix4x4Converter());

            // Write JSON to file
            using var streamWriter = new StreamWriter(writeStream);
            await streamWriter.WriteAsync(animationJson);

            // Finally (just for good practice), switch back to the main thread.
            await UniTask.SwitchToMainThread();
        }
        #endregion

        #region Loading
        // Invokes UniTask to load the animation asynchronously.
        public void OpenOpenPanel()
            => StandaloneFileBrowser.OpenFilePanelAsync("Open Animation", "", "json", false, paths => LoadAnimationAsync(paths).Forget());

        private async UniTask LoadAnimationAsync(string[] paths)
        {
            if (paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
            {
                Debug.Log("Open operation cancelled or invalid path.");
                return;
            }

            // Switch to thread pool for reading and deserializing
            await UniTask.SwitchToThreadPool();

            var file = new FileInfo(paths[0]);
            var readStream = file.OpenRead();

            // Read JSON from file
            using var streamReader = new StreamReader(readStream);
            var animationJson = await streamReader.ReadToEndAsync();
            
            // Deserialize JSON to Animation object
            var animation = JsonConvert.DeserializeObject<Animation>(animationJson, new Matrix4x4Converter());

            // Switch back to main thread
            // This is required because Unity interactions can only be done on main thread.
            await UniTask.SwitchToMainThread();

            // Load the animation into the timeline controller.
            timelineController.LoadAnimation(animation);
        }
        #endregion
    }
}
