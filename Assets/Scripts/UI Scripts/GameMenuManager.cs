using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TMPro;

public class GameMenuManager : MonoBehaviour {
    [SerializeField] private GameObject menu, rosMenu, debugMenu, voiceMenu, instructionText;
    [SerializeField] private Interactable followToggle, attachToggle, navToggle;
    [SerializeField] private TMP_Text joyHandedness;
    [SerializeField] private RadialView follow;

    private JoystickControl joystick;
    private bool navigating = false;

    private void OnEnable() {
        SpeechManager.AddListener("menu", toggleMenu, true);
        SpeechManager.AddListener("toggle navigation", toggleNavigation, true);
    }

    private void OnDisable() {
        SpeechManager.RemoveListener("menu", toggleMenu);
        SpeechManager.RemoveListener("toggle navigation", toggleNavigation);
    }

    private void Start() {
        joystick = FindObjectOfType<JoystickControl>();
        followToggle.IsToggled = follow.enabled;
        attachToggle.IsToggled = joystick.attachToHand;
        joyHandedness.text = joystick.lhand ? "Left" : "Right";
        if (instructionText.activeSelf) {
            navToggle.IsToggled = true;
            navigating = true;
        } else CoreServices.SpatialAwarenessSystem.Disable();

        if (!PlayerPrefs.HasKey("Updated")) {
            print("###=== Game Updated ===###");
            PlayerPrefs.SetInt("Updated", 1);
        }
    }

#if UNITY_EDITOR
    private void Update() {
        if (Input.GetKeyDown(KeyCode.M)) toggleMenu();
    }
#endif

    private void toggleMenu() { menu.SetActive(!menu.activeSelf); }
    
    public void toggleMenuFollow() { follow.enabled = !follow.enabled; }

    public void toggleROSMenu() { rosMenu.SetActive(!rosMenu.activeSelf); }

    public void toggleDebugMenu() { debugMenu.SetActive(!debugMenu.activeSelf); }

    public void toggleVoiceMenu() { voiceMenu.SetActive(!voiceMenu.activeSelf); }

    public void setHand() {
        if (joyHandedness.text == "Left") {
            joyHandedness.text = "Right";
            joystick.lhand = false;
        } else {
            joyHandedness.text = "Left";
            joystick.lhand = true;
        }
    }

    public void toggleNavigation() {
        if (navigating) {
            CoreServices.SpatialAwarenessSystem.Disable();
            instructionText.SetActive(false);
            navToggle.IsToggled = false;
            navigating = false;
        } else {
            CoreServices.SpatialAwarenessSystem.Enable();
            instructionText.SetActive(true);
            navToggle.IsToggled = true;
            navigating = true;
        }
    }

    public void exitApp() { Application.Quit(); }
}