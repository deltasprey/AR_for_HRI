using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

public class LocMarkerManager : MonoBehaviour, IMixedRealitySpeechHandler {
    public GameObject simpleGUI, complexGUI;
    public Material sphere;
    public Transform localiser, worldMarker;
    public Slider simpleSlider, complexSlider;
    public TextMeshProUGUI simpleSliderText, complexSliderText;
    public TMP_Dropdown moveStep, rotateStep;
    public TMP_InputField moveX, moveY, moveZ, rotX, rotY, rotZ;
    public Button simpleSaveButton, complexSaveButton;

    Transform player;
    readonly float[] moveAmounts = { 0.001f, 0.005f, 0.01f, 0.05f, 0.1f, 0.5f };
    readonly uint[] rotateAmounts = { 1, 2, 5, 10, 15, 30, 45, 90 };
    float moveAmount, localiserScale;
    uint rotateAmount;
    bool track = false, trackScale = true;

    void Start() {
        player = Camera.main.transform;

        localiserScale = localiser.localScale.x * 100;
        simpleSlider.value = localiserScale;
        complexSlider.value = localiserScale;
        simpleSliderText.text = simpleSlider.value.ToString();
        complexSliderText.text = complexSlider.value.ToString();
        moveAmount = moveAmounts[moveStep.value];
        rotateAmount = rotateAmounts[rotateStep.value];
        StartCoroutine(alphaUp());
    }

    void Update() {
        if (localiser.localScale.x * 100 != localiserScale && trackScale) {
            simpleSlider.value = localiserScale;
            complexSlider.value = localiserScale;
            simpleSliderText.text = simpleSlider.value.ToString();
            complexSliderText.text = complexSlider.value.ToString();
        }
        
        if (track) {
            if (!rotX.isFocused) {
                rotX.text = localiser.eulerAngles.x.ToString("F3");
            }
            if (!rotY.isFocused) {
                rotY.text = (localiser.eulerAngles.y - worldMarker.eulerAngles.y).ToString("F3");
            }
            if (!rotZ.isFocused) {
                rotZ.text = localiser.eulerAngles.z.ToString("F3");
            }
        }

        transform.position = worldMarker.position;
        transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));
        transform.forward *= -1;
        simpleGUI.transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));
        simpleGUI.transform.forward *= -1;
        complexGUI.transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));
        complexGUI.transform.forward *= -1;
    }

    private void OnEnable() {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this);
    }

    private void OnDisable() {
        try {
            CoreServices.InputSystem.UnregisterHandler<IMixedRealitySpeechHandler>(this);
        } catch { }
    }

    void IMixedRealitySpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData) {
        if (eventData.Command.Keyword.ToLower() == "track marker") {
            track = true;
        } else if (eventData.Command.Keyword.ToLower() == "lock marker") {
            track = false;
        }
    }

    public void viewSphere() {
        StopCoroutine(alphaDown());
        StartCoroutine(alphaUp());
        localiser.GetComponent<SphereCollider>().enabled = true;
        localiser.GetComponent<Outline>().OutlineWidth = 5;
    }

    public void viewFrame() {
        StopCoroutine(alphaUp());
        StartCoroutine(alphaDown());
        localiser.GetComponent<SphereCollider>().enabled = false;
        localiser.GetComponent<Outline>().OutlineWidth = -1;
        localiser.GetComponent<Outline>().enabled = false;
    }

    public void adjustScale() {
        float newScale;
        if (simpleGUI.activeSelf) {
            newScale = simpleSlider.value/100;
            complexSlider.value = simpleSlider.value;
        } else {
            newScale = complexSlider.value/100;
            simpleSlider.value = complexSlider.value;
        }
        localiser.localScale = Vector3.one * newScale;
        simpleSliderText.text = simpleSlider.value.ToString();
        complexSliderText.text = complexSlider.value.ToString();
        localiserScale = newScale * 100;
    }

    public void sliderSelect() { trackScale = false; }
    public void sliderDeselect() { trackScale = true; }

    public void viewSimpleControls() {
        simpleGUI.SetActive(true);
        complexGUI.SetActive(false);
    }

    public void viewComplexControls() {
        simpleGUI.SetActive(false);
        complexGUI.SetActive(true);
    }

    public void changeMoveAmount() {
        moveAmount = moveAmounts[moveStep.value];
    }

    public void changeRotateAmount() {
        rotateAmount = rotateAmounts[rotateStep.value];
    }

    public void increaseMoveAmount() {
        if (moveStep.value < moveAmounts.Length - 1) {
            moveAmount = moveAmounts[++moveStep.value];
        }
    }

    public void decreaseMoveAmount() {
        if (moveStep.value > 0) {
            moveAmount = moveAmounts[--moveStep.value];
        }
    }

    public void increaseRotateAmount() {
        if (rotateStep.value < moveAmounts.Length - 1) {
            rotateAmount = rotateAmounts[++rotateStep.value];
        }
    }

    public void decreaseRotateAmount() {
        if (rotateStep.value > 0) {
            rotateAmount = rotateAmounts[--rotateStep.value];
        }
    }

    public void changeMoveX() {
        if (rotX.isFocused && float.TryParse(moveX.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float xShift)) {
            localiser.position = new Vector3(worldMarker.position.x + xShift, localiser.position.y, localiser.position.z);
        }
    }

    public void moveXPlus() {
        localiser.Translate(Vector3.right * moveAmount);
        moveX.text = (localiser.position.x - worldMarker.position.x).ToString();
    }

    public void moveXMinus() {
        localiser.Translate(Vector3.left * moveAmount);
        moveX.text = (localiser.position.x - worldMarker.position.x).ToString();
    }

    public void changeMoveY() {
        if (rotY.isFocused && float.TryParse(moveY.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float yShift)) {
            localiser.position = new Vector3(localiser.position.x, worldMarker.position.y + yShift, localiser.position.z);
        }
    }

    public void moveYPlus() {
        localiser.Translate(Vector3.up * moveAmount);
        moveY.text = (localiser.position.y - worldMarker.position.y).ToString();
    }

    public void moveYMinus() {
        localiser.Translate(Vector3.down * moveAmount);
        moveY.text = (localiser.position.y - worldMarker.position.y).ToString();
    }

    public void changeMoveZ() {
        if (rotZ.isFocused && float.TryParse(moveZ.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float zShift)) {
            localiser.position = new Vector3(localiser.position.x, localiser.position.y, worldMarker.position.z + zShift);
        }
    }

    public void moveZPlus() {
        localiser.Translate(Vector3.forward * moveAmount);
        moveZ.text = (localiser.position.z - worldMarker.position.z).ToString();
    }

    public void moveZMinus() {
        localiser.Translate(Vector3.back * moveAmount);
        moveZ.text = (localiser.position.z - worldMarker.position.z).ToString();
    }

    public void changeRotateX() {
        if (float.TryParse(rotX.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float xShift)) {
            localiser.rotation = Quaternion.Euler(xShift, localiser.eulerAngles.y, localiser.eulerAngles.z);
        }
    }

    public void rotateXPlus() {
        localiser.Rotate(Vector3.right * rotateAmount);
        rotX.text = localiser.eulerAngles.x.ToString("F3");
        
    }

    public void rotateXMinus() {
        localiser.Rotate(Vector3.left * rotateAmount);
        rotX.text = localiser.eulerAngles.x.ToString("F3");
    }

    public void changeRotateY() {
        if (float.TryParse(rotY.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float yShift)) {
            localiser.rotation = Quaternion.Euler(localiser.eulerAngles.x, worldMarker.eulerAngles.y + yShift, localiser.eulerAngles.z);
        }
    }

    public void rotateYPlus() {
        localiser.Rotate(Vector3.up * rotateAmount);
        rotY.text = (localiser.eulerAngles.y - worldMarker.eulerAngles.y).ToString("F3");
    }

    public void rotateYMinus() {
        localiser.Rotate(Vector3.down * rotateAmount);
        rotY.text = (localiser.eulerAngles.y - worldMarker.eulerAngles.y).ToString("F3");
    }

    public void changeRotateZ() {
        if (float.TryParse(rotZ.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float zShift)) {
            localiser.rotation = Quaternion.Euler(localiser.eulerAngles.x, localiser.eulerAngles.y, zShift);
        }
    }

    public void rotateZPlus() {
        localiser.Rotate(Vector3.forward * rotateAmount);
        rotZ.text = localiser.eulerAngles.z.ToString("F3");
    }

    public void rotateZMinus() {
        localiser.Rotate(Vector3.back * rotateAmount);
        rotZ.text = localiser.eulerAngles.z.ToString("F3");
    }

    public void save() {
        simpleSaveButton.interactable = false;
        complexSaveButton.interactable = false;
        PlayerPrefs.SetFloat("moveX", localiser.position.x - worldMarker.position.x);
        PlayerPrefs.SetFloat("moveY", localiser.position.y - worldMarker.position.y);
        PlayerPrefs.SetFloat("moveZ", localiser.position.z - worldMarker.position.z);
        PlayerPrefs.SetFloat("rotX", localiser.eulerAngles.x);
        PlayerPrefs.SetFloat("rotY", (localiser.eulerAngles.y - worldMarker.eulerAngles.y));
        PlayerPrefs.SetFloat("rotZ", localiser.eulerAngles.z);
        PlayerPrefs.Save();
        StartCoroutine(saveDisplay());
    }

    public void load() {
        if (PlayerPrefs.HasKey("moveX")) {
            float xShift = PlayerPrefs.GetFloat("moveX");
            float yShift = PlayerPrefs.GetFloat("moveY");
            float zShift = PlayerPrefs.GetFloat("moveZ");
            float xRot = PlayerPrefs.GetFloat("rotX");
            float yRot = PlayerPrefs.GetFloat("rotY");
            float zRot = PlayerPrefs.GetFloat("rotZ");
            localiser.SetPositionAndRotation(worldMarker.position + new Vector3(xShift, yShift, zShift), Quaternion.Euler(xRot, worldMarker.eulerAngles.y + yRot, zRot));

            moveX.text = (localiser.position.x - worldMarker.position.x).ToString();
            moveY.text = (localiser.position.y - worldMarker.position.y).ToString();
            moveZ.text = (localiser.position.z - worldMarker.position.z).ToString();
            rotX.text = localiser.eulerAngles.x.ToString();
            rotY.text = (localiser.eulerAngles.y - worldMarker.eulerAngles.y).ToString();
            rotZ.text = localiser.eulerAngles.z.ToString();
        }
    }

    IEnumerator alphaUp() {
        Color color = sphere.color;
        while (color.a < 1) {
            color.a += 0.05f;
            sphere.color = color;
            yield return new WaitForSeconds(0.005f);
        }
    }

    IEnumerator alphaDown() {
        Color color = sphere.color;
        while (color.a > 0) {
            color.a -= 0.05f;
            sphere.color = color;
            yield return new WaitForSeconds(0.005f);
        }
    }

    IEnumerator saveDisplay() {
        simpleSaveButton.GetComponentInChildren<TextMeshProUGUI>().text = "Saving...";
        complexSaveButton.GetComponentInChildren<TextMeshProUGUI>().text = "Saving...";
        yield return new WaitForSeconds(0.5f);
        simpleSaveButton.GetComponentInChildren<TextMeshProUGUI>().text = "Save Offset";
        complexSaveButton.GetComponentInChildren<TextMeshProUGUI>().text = "Save Offset";
        simpleSaveButton.interactable = true;
        complexSaveButton.interactable = true;
    }
}
