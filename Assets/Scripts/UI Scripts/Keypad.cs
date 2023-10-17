using System;
using TMPro;
using UnityEngine;

[Serializable]
public class InputLocMapping {
    public TMP_InputField inputField;
    public Vector3 keypadPos;
}

public class Keypad : MonoBehaviour {
    [SerializeField] private GameObject keypad;
    [SerializeField] private InputLocMapping[] inputKeypadPos;
    private TMP_InputField inputField;

    public void selected(TMP_InputField field) {
        keypad.SetActive(true);
        inputField = field;
        foreach(InputLocMapping mapped in inputKeypadPos) {
            if (mapped.inputField == field) {
                keypad.transform.localPosition = mapped.keypadPos;
                break;
            }
        }
    }

    public void close() { keypad.SetActive(false); }

    public void input(int number) {
        if (inputField.text.Length < (inputField.characterLimit > 0 ? inputField.characterLimit : 32)) {
            if (number >= 0) inputField.text += number.ToString();
            else inputField.text += ".";
        }
    }

    public void backspace() { 
        if (inputField.text.Length > 0) inputField.text = inputField.text[..(inputField.text.Length - 1)]; 
    }

    public void clear() { inputField.text = ""; }
}