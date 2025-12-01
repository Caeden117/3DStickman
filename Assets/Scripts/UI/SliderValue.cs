using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Slider))]
public class SliderValue : MonoBehaviour
{
    [SerializeField]
    protected Text _text = null;

    protected Slider _slider = null;

    private void Start()
    {
        _slider = GetComponent<Slider>();
    }

    private void Update()
    {
        // Update every frame in case value was updated without notify.
        if (_text != null)
        {
            if (_slider.wholeNumbers)
            {
                _text.text = _slider.value.ToString("0");
            }
            else
            {
                _text.text = _slider.value.ToString("F3");
            }
        }
    }
}
