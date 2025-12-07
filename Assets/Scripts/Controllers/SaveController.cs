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
        [SerializeField] private HistoryController historyController;

        private bool isOperating = false;

        private void Update()
        {
            // Early return if already performing an operation (should prevent duplicates).
            if (isOperating) return;

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
        {
            if (isOperating) return;

            StandaloneFileBrowser.SaveFilePanelAsync("Save Animation", "", "animation.json", "json", path => SaveAnimationAsync(path).Forget());
        }

        // Asynchronously saves the current animation to the specified path.
        private async UniTask SaveAnimationAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Save operation cancelled or invalid path.");
                return;
            }

            isOperating = true;

            // We are executing on the main Unity thread by default. Switch to a thread pool so these operations don't cause lag.
            await UniTask.SwitchToThreadPool();

            // Serialize loaded animation to JSON
            var animation = timelineController.LoadedAnimation;
            var animationJson = JsonConvert.SerializeObject(animation, Formatting.None, new Matrix4x4Converter());

            await File.WriteAllTextAsync(path, animationJson);

            // Finally (just for good practice), switch back to the main thread.
            await UniTask.SwitchToMainThread();

            isOperating = false;
        }
        #endregion

        #region Loading
        // Invokes UniTask to load the animation asynchronously.
        public void OpenOpenPanel()
        {
            if (isOperating) return;

            StandaloneFileBrowser.OpenFilePanelAsync("Open Animation", "", "json", false, paths => LoadAnimationAsync(paths).Forget());
        }

        private async UniTask LoadAnimationAsync(string[] paths)
        {
            if (paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
            {
                Debug.Log("Open operation cancelled or invalid path.");
                return;
            }

            isOperating = true;

            // Switch to thread pool for reading and deserializing
            await UniTask.SwitchToThreadPool();

            // Read JSON from file
            var animationJson = await File.ReadAllTextAsync(paths[0]);
            
            // Deserialize JSON to Animation object
            var animation = JsonConvert.DeserializeObject<Animation>(animationJson, new Matrix4x4Converter());

            // Switch back to main thread
            // This is required because Unity interactions can only be done on main thread.
            await UniTask.SwitchToMainThread();

            // Load the animation into the timeline controller.
            // Clears command history since all previous commands are no longer relevant.
            historyController.Clear();
            timelineController.LoadAnimation(animation);
            isOperating = false;
        }
        #endregion
    }
}
