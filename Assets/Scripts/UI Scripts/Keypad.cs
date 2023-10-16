using TMPro;
using UnityEngine;

public class Keypad : MonoBehaviour {
    [SerializeField] GameObject keypad;
    private TMP_Text inputField;

    public void selected(TMP_Text field) {
        keypad.SetActive(true);
        inputField = field;
        if (inputField.transform.name == "IP Text") keypad.transform.position = new Vector3(75, 25);
        else keypad.transform.position = new Vector3(75, 0);
    }

    public void close() { keypad.SetActive(false); }

    public void input(int number) {
        if (number > 0) inputField.text += number.ToString();
        else inputField.text += ".";
    }

    public void backspace() {
        inputField.text = inputField.text[..(inputField.text.Length - 1)];
    }
}
