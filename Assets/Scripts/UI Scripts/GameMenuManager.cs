using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour {
    public GameObject menu, instructionText;
    public Transform player;
    public Toggle spatialToggle, navToggle;
    public float spawnDistance = 2;
    public bool follow = false;

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
        CoreServices.SpatialAwarenessSystem.Disable();

        if (!PlayerPrefs.HasKey("Updated")) {
            print("###=== Game Updated ===###");
            PlayerPrefs.SetInt("Updated", 1);
        }
    }

    private void Update() {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.M)) {
            toggleMenu();
        }
#endif
        if (menu.activeSelf) {
            menu.transform.LookAt(player.position);
            menu.transform.forward *= -1;
            if (follow)
                menu.transform.position = player.position + player.forward.normalized * spawnDistance;
        }
    }

    private void toggleMenu() {
        menu.SetActive(!menu.activeSelf);
        menu.transform.position = player.position + player.forward.normalized * spawnDistance;
    }

    public void toggleSpatialAwareness() {
        if (spatialToggle.isOn) CoreServices.SpatialAwarenessSystem.Enable();
        else {
            CoreServices.SpatialAwarenessSystem.Disable();
            if (navigating) toggleNavigation();
        }
    }

    public void toggleMenuFollow() { follow = !follow; }

    public void toggleNavigation() {
        if (navigating) {
            CoreServices.SpatialAwarenessSystem.Disable();
            instructionText.SetActive(false);
            spatialToggle.isOn = false;
            navToggle.isOn = false;
            navigating = false;
        } else {
            CoreServices.SpatialAwarenessSystem.Enable();
            instructionText.SetActive(true);
            spatialToggle.isOn = true;
            navToggle.isOn = true;
            navigating = true;
        }
    }

    public void exitApp() { Application.Quit(); }
}