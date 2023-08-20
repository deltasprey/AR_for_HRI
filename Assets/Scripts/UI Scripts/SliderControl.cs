using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderControl : MonoBehaviour {
    public Slider slider;
    public TextMeshProUGUI text;
    public Material cube;

    private void Start() {
        text.text = slider.value.ToString();
    }

    public void ChangeSensitivity() {
        text.text = slider.value.ToString();
        Color color = cube.color;
        color.a = slider.value;
        cube.color = color;
    }
}
