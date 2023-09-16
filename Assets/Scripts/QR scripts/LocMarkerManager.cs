using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Unity.VisualScripting;

public class LocMarkerManager : MonoBehaviour, IMixedRealitySpeechHandler {
    public QRTracking.QRCode qr;
    public GameObject simpleGUI, complexGUI;
    public Material sphere;
    public Transform localiser, worldMarker, safetyZone;
    public Slider simpleScaleSlider, complexScaleSlider, simpleSafetySlider, complexSafetySlider;
    public TextMeshProUGUI simpleScaleSliderText, complexScaleSliderText, simpleSafetySliderText, complexSafetySliderText;
    public TMP_Dropdown moveStep, rotateStep;
    public TMP_InputField moveX, moveY, moveZ, rotX, rotY, rotZ;
    public Button simpleSaveButton, complexSaveButton;

    private Transform player, origParent;
    private CmdVelControl rosPose;
    private Vector3 position;
    private Quaternion rotation;
    private Matrix4x4 transformationMatrix;

    private readonly float[] moveAmounts = { 0.001f, 0.005f, 0.01f, 0.05f, 0.1f, 0.5f };
    private readonly uint[] rotateAmounts = { 1, 2, 5, 10, 15, 30, 45, 90 };
    private float moveAmount, localiserScale, safetyScale, offsetTheta;
    private uint rotateAmount;
    private bool track = false, trackScale = true, initialised = false;

    private void Start() {
        // Marker positioning initialisation
        player = Camera.main.transform;
        origParent = transform.parent;
        rosPose = FindObjectOfType<CmdVelControl>();
        //print(rosPose.linearSpeed);
        Invoke(nameof(markerMoved), 0.5f);

        // UI initialisation
        localiserScale = localiser.localScale.x * 100;
        simpleScaleSlider.value = localiserScale;
        complexScaleSlider.value = localiserScale;
        simpleScaleSliderText.text = simpleScaleSlider.value.ToString();
        complexScaleSliderText.text = complexScaleSlider.value.ToString();

        safetyScale = safetyZone.localScale.x * localiserScale/100;
        simpleSafetySlider.value = safetyScale * 10;
        complexSafetySlider.value = safetyScale * 10;
        simpleSafetySliderText.text = safetyScale.ToString();
        complexSafetySliderText.text = safetyScale.ToString();

        moveAmount = moveAmounts[moveStep.value];
        rotateAmount = rotateAmounts[rotateStep.value];
        StartCoroutine(alphaUp());

        // QR code reading
        //qr = GetComponentInParent<QRTracking.QRCode>();
        print(qr.CodeText);
        if (qr.CodeText.Length > 0 && qr.CodeText[0] == '(' && qr.CodeText[^1] == ')') {
            if (qr.CodeText.CountIndices(',') == 2) {
                print("Offset");
                string[] codeVals = qr.CodeText[1..^1].Split(',');
                QROffset(float.Parse(codeVals[0]), float.Parse(codeVals[1]), float.Parse(codeVals[2]));
            } else if (qr.CodeText.CountIndices(',') == 5) {
                print("Offset and rotation");
                string[] codeVals = qr.CodeText[1..^1].Split(',');
                QROffsetRotation(float.Parse(codeVals[0]), float.Parse(codeVals[1]), float.Parse(codeVals[2]),
                                 float.Parse(codeVals[3]), float.Parse(codeVals[4]), float.Parse(codeVals[5]));
            }
        } 
    }

    private void Update() {
        if (localiser.localScale.x * 100 != localiserScale && trackScale) {
            simpleScaleSlider.value = localiserScale;
            complexScaleSlider.value = localiserScale;
            simpleScaleSliderText.text = simpleScaleSlider.value.ToString();
            complexScaleSliderText.text = complexScaleSlider.value.ToString();

            simpleSafetySlider.value = safetyScale * 10;
            complexSafetySlider.value = safetyScale * 10;
            simpleSafetySliderText.text = safetyScale.ToString();
            complexSafetySliderText.text = safetyScale.ToString();
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
        CmdVelControl.msgValueChanged += moveMarker;
    }

    private void OnDisable() {
        try {
            CoreServices.InputSystem.UnregisterHandler<IMixedRealitySpeechHandler>(this);
        } catch { }
        CmdVelControl.msgValueChanged -= moveMarker;
    }

    private void QROffset(float x, float y, float z) {
        localiser.position = new Vector3(worldMarker.position.x + x, worldMarker.position.y + y, worldMarker.position.z + z);
        moveX.text = (x - worldMarker.position.x).ToString();
        moveY.text = (y - worldMarker.position.y).ToString();
        moveZ.text = (z - worldMarker.position.z).ToString();
    }

    private void QROffsetRotation(float x, float y, float z, float rx, float ry, float rz) {
        localiser.SetPositionAndRotation(new Vector3(worldMarker.position.x + x, worldMarker.position.y + y, worldMarker.position.z + z), Quaternion.Euler(rx, ry, rz));
        moveX.text = (x - worldMarker.position.x).ToString();
        moveY.text = (y - worldMarker.position.y).ToString();
        moveZ.text = (z - worldMarker.position.z).ToString();
        rotX.text = rx.ToString();
        rotY.text = (ry - worldMarker.eulerAngles.y).ToString();
        rotZ.text = rz.ToString();
    }

    // Robot position changed event callback
    private void moveMarker(float x, float z, float theta) {
        if (initialised) {
            worldMarker.SetParent(localiser);
            //Vector3 position = new(x + offset.x, offset.y, z + offset.z);
            position = transformationMatrix.MultiplyPoint(new Vector3(x, 0, z));
            rotation = Quaternion.Euler(0, theta + offsetTheta, 0); //* Mathf.Rad2Deg - 90
            localiser.SetPositionAndRotation(position, rotation);
            worldMarker.SetParent(origParent);
        } else {
            markerMoved();
        }
    }

    // Calculate offset between virtual marker and robot pose
    // Must update whenever the marker is moved
    public void markerMoved() {
        var (x, z, theta) = rosPose.initPos();
        if (theta != 404 && x != 0 && z != 0) {
            // Coordinate frame transform
            Quaternion botRot = Quaternion.Euler(0, localiser.rotation.eulerAngles.y, 0);
            Quaternion rotationAB = Quaternion.Inverse(Quaternion.Euler(0, theta, 0)) * botRot;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, rotationAB, Vector3.one);
            transformationMatrix = Matrix4x4.Translate(localiser.position - new Vector3(x, 0 , z)) * rotationMatrix;
            offsetTheta = localiser.rotation.eulerAngles.y - theta;

            //offset = new(localiser.position.x - x, localiser.position.y, localiser.position.z - z);
            //offsetTheta = localiser.rotation.z - theta;
            initialised = true;
        }
    }

    void IMixedRealitySpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData) {
        if (eventData.Command.Keyword.ToLower() == "track marker") {
            track = true;
        } else if (eventData.Command.Keyword.ToLower() == "lock marker") {
            track = false;
        }
    }

    #region Marker Object UI Control
    // View virtual marker in sphere mode
    public void viewSphere() {
        StopCoroutine(alphaDown());
        StartCoroutine(alphaUp());
        localiser.GetComponent<SphereCollider>().enabled = true;
        localiser.GetComponent<Outline>().OutlineWidth = 5;
    }

    // View virtual marker in coordinate frame mode
    public void viewFrame() {
        StopCoroutine(alphaUp());
        StartCoroutine(alphaDown());
        localiser.GetComponent<SphereCollider>().enabled = false;
        localiser.GetComponent<Outline>().OutlineWidth = -1;
        localiser.GetComponent<Outline>().enabled = false;
    }

    // Change the size of the virtual marker
    public void adjustScale() {
        if (simpleGUI.activeSelf) {
            localiserScale = simpleScaleSlider.value;
            complexScaleSlider.value = simpleScaleSlider.value;
        } else {
            localiserScale = complexScaleSlider.value;
            simpleScaleSlider.value = complexScaleSlider.value;
        }
        localiser.localScale = Vector3.one * localiserScale/100;
        simpleScaleSliderText.text = localiserScale.ToString();
        complexScaleSliderText.text = localiserScale.ToString();
        adjustSafety();
    }

    // Change the size of the safety zone
    public void adjustSafety() {
        if (simpleGUI.activeSelf) {
            safetyScale = simpleSafetySlider.value/10;
            complexSafetySlider.value = simpleSafetySlider.value;
        } else {
            safetyScale = complexSafetySlider.value/10;
            simpleSafetySlider.value = complexSafetySlider.value;
        }
        safetyZone.localScale = Vector3.one * safetyScale/localiserScale * 100;
        simpleSafetySliderText.text = safetyScale.ToString();
        complexSafetySliderText.text = safetyScale.ToString();
    }

    public void sliderSelect() { trackScale = false; }
    public void sliderDeselect() { trackScale = true; }
    #endregion

    #region Complex UI Button Callbacks
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
            markerMoved();
        }
    }

    public void moveXPlus() {
        localiser.Translate(Vector3.right * moveAmount);
        moveX.text = (localiser.position.x - worldMarker.position.x).ToString();
        markerMoved();
    }

    public void moveXMinus() {
        localiser.Translate(Vector3.left * moveAmount);
        moveX.text = (localiser.position.x - worldMarker.position.x).ToString();
        markerMoved();
    }

    public void changeMoveY() {
        if (rotY.isFocused && float.TryParse(moveY.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float yShift)) {
            localiser.position = new Vector3(localiser.position.x, worldMarker.position.y + yShift, localiser.position.z);
            markerMoved();
        }
    }

    public void moveYPlus() {
        localiser.Translate(Vector3.up * moveAmount);
        moveY.text = (localiser.position.y - worldMarker.position.y).ToString();
        markerMoved();
    }

    public void moveYMinus() {
        localiser.Translate(Vector3.down * moveAmount);
        moveY.text = (localiser.position.y - worldMarker.position.y).ToString();
        markerMoved();
    }

    public void changeMoveZ() {
        if (rotZ.isFocused && float.TryParse(moveZ.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float zShift)) {
            localiser.position = new Vector3(localiser.position.x, localiser.position.y, worldMarker.position.z + zShift);
            markerMoved();
        }
    }

    public void moveZPlus() {
        localiser.Translate(Vector3.forward * moveAmount);
        moveZ.text = (localiser.position.z - worldMarker.position.z).ToString();
        markerMoved();
    }

    public void moveZMinus() {
        localiser.Translate(Vector3.back * moveAmount);
        moveZ.text = (localiser.position.z - worldMarker.position.z).ToString();
        markerMoved();
    }

    public void changeRotateX() {
        if (float.TryParse(rotX.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float xShift)) {
            localiser.rotation = Quaternion.Euler(xShift, localiser.eulerAngles.y, localiser.eulerAngles.z);
            markerMoved();
        }
    }

    public void rotateXPlus() {
        localiser.Rotate(Vector3.right * rotateAmount);
        rotX.text = localiser.eulerAngles.x.ToString("F3");
        markerMoved();
    }

    public void rotateXMinus() {
        localiser.Rotate(Vector3.left * rotateAmount);
        rotX.text = localiser.eulerAngles.x.ToString("F3");
        markerMoved();
    }

    public void changeRotateY() {
        if (float.TryParse(rotY.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float yShift)) {
            localiser.rotation = Quaternion.Euler(localiser.eulerAngles.x, worldMarker.eulerAngles.y + yShift, localiser.eulerAngles.z);
            markerMoved();
        }
    }

    public void rotateYPlus() {
        localiser.Rotate(Vector3.up * rotateAmount);
        rotY.text = (localiser.eulerAngles.y - worldMarker.eulerAngles.y).ToString("F3");
        markerMoved();
    }

    public void rotateYMinus() {
        localiser.Rotate(Vector3.down * rotateAmount);
        rotY.text = (localiser.eulerAngles.y - worldMarker.eulerAngles.y).ToString("F3");
        markerMoved();
    }

    public void changeRotateZ() {
        if (float.TryParse(rotZ.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float zShift)) {
            localiser.rotation = Quaternion.Euler(localiser.eulerAngles.x, localiser.eulerAngles.y, zShift);
            markerMoved();
        }
    }

    public void rotateZPlus() {
        localiser.Rotate(Vector3.forward * rotateAmount);
        rotZ.text = localiser.eulerAngles.z.ToString("F3");
        markerMoved();
    }

    public void rotateZMinus() {
        localiser.Rotate(Vector3.back * rotateAmount);
        rotZ.text = localiser.eulerAngles.z.ToString("F3");
        markerMoved();
    }
    #endregion

    #region Saving and Loading
    // Store localisation marker position and rotation in PlayerPrefs
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

    // Load previously saved localisation marker position and rotation in PlayerPrefs
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
            markerMoved();
        }
    }
    #endregion

    #region Animations
    // Hide coordinate frame of the virtual marker
    private IEnumerator alphaUp() {
        Color color = sphere.color;
        while (color.a < 1) {
            color.a += 0.05f;
            sphere.color = color;
            yield return new WaitForSeconds(0.005f);
        }
    }

    // Show coordinate frame of the virtual marker
    private IEnumerator alphaDown() {
        Color color = sphere.color;
        while (color.a > 0) {
            color.a -= 0.05f;
            sphere.color = color;
            yield return new WaitForSeconds(0.005f);
        }
    }

    // Visual indicator for save button being pressed
    private IEnumerator saveDisplay() {
        simpleSaveButton.GetComponentInChildren<TextMeshProUGUI>().text = "Saving...";
        complexSaveButton.GetComponentInChildren<TextMeshProUGUI>().text = "Saving...";
        yield return new WaitForSeconds(0.5f);
        simpleSaveButton.GetComponentInChildren<TextMeshProUGUI>().text = "Save Offset";
        complexSaveButton.GetComponentInChildren<TextMeshProUGUI>().text = "Save Offset";
        simpleSaveButton.interactable = true;
        complexSaveButton.interactable = true;
    }
    #endregion
}