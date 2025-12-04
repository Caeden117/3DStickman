using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stickman3D
{
    public class CreateObjectItemController : MonoBehaviour
    {
        [field: SerializeField]
        public Button Button { get; private set; }

        [SerializeField] private RawImage iconImage;
        [SerializeField] private TextMeshProUGUI nameText;

        public void SetObjectInformation(string objectName, RenderTexture objectPreview)
        {
            nameText.text = objectName;
            iconImage.texture = objectPreview;
        }
    }
}
