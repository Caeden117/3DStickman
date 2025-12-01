using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Stickman3D
{
    public class KeyframeEditorController : MonoBehaviour
    {
        [SerializeField]
        private RectTransform empty = null;

        [SerializeField]
        private RectTransform controls = null;

        [SerializeField]
        private InputField inputFieldTime = null;

        [SerializeField]
        private InputField inputFieldTranslationX = null;

        [SerializeField]
        private InputField inputFieldTranslationY = null;

        [SerializeField]
        private InputField inputFieldTranslationZ = null;

        [SerializeField]
        private InputField inputFieldRotationX = null;

        [SerializeField]
        private InputField inputFieldRotationY = null;

        [SerializeField]
        private InputField inputFieldRotationZ = null;

        [SerializeField]
        private InputField inputFieldScaleX = null;

        [SerializeField]
        private InputField inputFieldScaleY = null;

        [SerializeField]
        private InputField inputFieldScaleZ = null;

        public UnityEvent<string, Keyframe, Keyframe> OnKeyframeChanged = new UnityEvent<string, Keyframe, Keyframe>();

        private string currentPath = "";
        private Keyframe currentKeyframe = new Keyframe();

        public void SetCurrent(string path, Keyframe keyframe)
        {
            currentPath = path;
            currentKeyframe = keyframe;
            RefreshControls();
        }

        public string GetCurrentPath()
        {
            return currentPath;
        }

        public Keyframe GetCurrentKeyframe()
        {
            return currentKeyframe;
        }

        private void Start()
        {
            inputFieldTime.onSubmit.AddListener(delegate { UpdateCurrent(); });
            inputFieldTranslationX.onSubmit.AddListener(delegate { UpdateCurrent(); });
            inputFieldTranslationY.onSubmit.AddListener(delegate { UpdateCurrent(); });
            inputFieldTranslationZ.onSubmit.AddListener(delegate { UpdateCurrent(); });
            inputFieldRotationX.onSubmit.AddListener(delegate { UpdateCurrent(); });
            inputFieldRotationY.onSubmit.AddListener(delegate { UpdateCurrent(); });
            inputFieldRotationZ.onSubmit.AddListener(delegate { UpdateCurrent(); });
            inputFieldScaleX.onSubmit.AddListener(delegate { UpdateCurrent(); });
            inputFieldScaleY.onSubmit.AddListener(delegate { UpdateCurrent(); });
            inputFieldScaleZ.onSubmit.AddListener(delegate { UpdateCurrent(); });

            RefreshControls();
        }

        private void UpdateCurrent()
        {
            if (currentPath.Length == 0)
            {
                return;
            }

            var newKeyframe = new Keyframe();
            var oldKeyframe = currentKeyframe;

            newKeyframe.Time = Mathf.Max(float.Parse(inputFieldTime.text), 0.0f);

            var translation = new Vector3(
                float.Parse(inputFieldTranslationX.text),
                float.Parse(inputFieldTranslationY.text),
                float.Parse(inputFieldTranslationZ.text));

            var rotation = Quaternion.Euler(new Vector3(
                float.Parse(inputFieldRotationX.text),
                float.Parse(inputFieldRotationY.text),
                float.Parse(inputFieldRotationZ.text)));

            var scale = new Vector3(
                Mathf.Max(float.Parse(inputFieldScaleX.text), 0.001f),
                Mathf.Max(float.Parse(inputFieldScaleY.text), 0.001f),
                Mathf.Max(float.Parse(inputFieldScaleZ.text), 0.001f));

            newKeyframe.Transform = Matrix4x4.TRS(translation, rotation, scale);

            currentKeyframe = newKeyframe;
            OnKeyframeChanged.Invoke(currentPath, oldKeyframe, newKeyframe);
        }

        private void RefreshControls()
        {
            if (currentPath.Length == 0)
            {
                empty.gameObject.SetActive(true);
                controls.gameObject.SetActive(false);
            }
            else
            {
                empty.gameObject.SetActive(false);
                controls.gameObject.SetActive(true);

                var time = currentKeyframe.Time;
                var translation = currentKeyframe.Transform.GetPosition();
                var rotation = currentKeyframe.Transform.rotation.eulerAngles;
                var scale = currentKeyframe.Transform.lossyScale;

                inputFieldTime.SetTextWithoutNotify(time.ToString());
                inputFieldTranslationX.SetTextWithoutNotify(translation.x.ToString());
                inputFieldTranslationY.SetTextWithoutNotify(translation.y.ToString());
                inputFieldTranslationZ.SetTextWithoutNotify(translation.z.ToString());
                inputFieldRotationX.SetTextWithoutNotify(rotation.x.ToString());
                inputFieldRotationY.SetTextWithoutNotify(rotation.y.ToString());
                inputFieldRotationZ.SetTextWithoutNotify(rotation.z.ToString());
                inputFieldScaleX.SetTextWithoutNotify(scale.x.ToString());
                inputFieldScaleY.SetTextWithoutNotify(scale.y.ToString());
                inputFieldScaleZ.SetTextWithoutNotify(scale.z.ToString());
            }
        }
    }
}
