using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class GameMenuManager : MonoBehaviour {
    [SerializeField] private GameObject menu, instructionText;
    [SerializeField] private Transform player;
    [SerializeField] private Interactable attachToggle, spatialToggle, navToggle;
    [SerializeField] private RadialView follow;
    [SerializeField] private float spawnDistance = 2;

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
        attachToggle.IsToggled = FindObjectOfType<JoystickControl>().attachToHand;
        if (instructionText.activeSelf) {
            spatialToggle.IsToggled = true;
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

    private void toggleMenu() {
        menu.SetActive(!menu.activeSelf);
        transform.position = player.position + player.forward.normalized * spawnDistance;
    }

    public void toggleSpatialAwareness() {
        if (spatialToggle.IsToggled) CoreServices.SpatialAwarenessSystem.Enable();
        else {
            CoreServices.SpatialAwarenessSystem.Disable();
            if (navigating) toggleNavigation();
        }
    }

    public void toggleMenuFollow() { follow.enabled = !follow.enabled; }

    public void toggleNavigation() {
        if (navigating) {
            CoreServices.SpatialAwarenessSystem.Disable();
            instructionText.SetActive(false);
            spatialToggle.IsToggled = false;
            navToggle.IsToggled = false;
            navigating = false;
        } else {
            CoreServices.SpatialAwarenessSystem.Enable();
            instructionText.SetActive(true);
            spatialToggle.IsToggled = true;
            navToggle.IsToggled = true;
            navigating = true;
        }
    }

    public void exitApp() { Application.Quit(); }
}