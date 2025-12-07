using System;
using Cysharp.Threading.Tasks;
using FFmpegOut;
using SFB;
using UnityEngine;
using UnityEngine.UI;

namespace Stickman3D
{
    public class RenderAnimationController : MonoBehaviour
    {
        private const int animationWidth = 1920;
        private const int animationHeight = 1080;
        private const string animationCameraPath = "Animation Camera";

        [SerializeField] private TimelineController timelineController;
        [SerializeField] private CameraCapture renderCameraPrefab;
        [SerializeField] private Button renderButton;

        private void Start()
        {
            if (!FFmpegPipe.IsAvailable)
            {
                Debug.LogWarning("FFmpeg is not available. Animation rendering is disabled.");
                renderButton.interactable = false;
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.R))
            {
                OpenRenderPanel();
            }
        }

        public void OpenRenderPanel()
        {
            if (!FFmpegPipe.IsAvailable) return;

            StandaloneFileBrowser.SaveFilePanelAsync("Save Animation", "", "animation.mp4", "mp4",
                p => RenderAnimationAsync(p).Forget());
        }

        public async UniTask RenderAnimationAsync(string path)
        {
            if (string.IsNullOrEmpty(path) || !FFmpegPipe.IsAvailable)
                return;

            // Reset timeline to first frame
            timelineController.IsPlaying = false;
            timelineController.CurrentFrame = 0;

            // Get animation details
            var animationLength = timelineController.LoadedAnimation.Length;
            var animationCameraNode = timelineController.GetNodeAtPath(animationCameraPath);
            var framerate = timelineController.Framerate;

            if (animationCameraNode == null)
            {
                Debug.LogError("Animation Camera not found in the timeline.");
                return;
            }

            var animationCamera = animationCameraNode.GetComponent<Camera>();

            // Initialize FFmpeg session
            using var ffmpegSession = FFmpegSession.CreateWithOutputPath(path,
                animationWidth,
                animationHeight,
                framerate,
                FFmpegPreset.H264Default);

            // Setup render camera
            var renderCameraCapture = Instantiate(renderCameraPrefab, animationCameraNode.transform, false);
            var renderCameraObject = renderCameraCapture.gameObject;
            var renderCameraFramerateController = renderCameraCapture.GetComponent<FrameRateController>();
            var renderCamera = renderCameraCapture.GetComponent<Camera>();

            renderCameraCapture.Session = ffmpegSession;
            renderCameraCapture.width = animationWidth;
            renderCameraCapture.height = animationHeight;
            renderCameraCapture.frameRate = framerate;
            
            renderCameraFramerateController.Framerate = framerate;
        
            renderCamera.fieldOfView = animationCamera.fieldOfView;

            // Start rendering frames
            renderCameraObject.SetActive(true);
            timelineController.IsPlaying = true;

            // Wait until rendering is complete
            await UniTask.WaitWhile(() => timelineController.IsPlaying);

            // Reset timeline to first frame
            timelineController.CurrentFrame = 0;

            // Destroy render camera
            // FFMpeg session will be disposed automatically
            DestroyImmediate(renderCameraObject);
            Debug.Log("Rendering completed: " + path);
        }
    }
}
