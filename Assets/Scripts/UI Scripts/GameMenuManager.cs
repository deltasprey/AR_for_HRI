using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour, IMixedRealitySpeechHandler {
    public GameObject menu, instructionText;
    public Transform player;
    public Toggle toggle;
    public float spawnDistance = 2;
    public bool follow = false;

    private bool navigating = false;

    private void OnEnable() { CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this); }

    private void OnDisable() {
        try { CoreServices.InputSystem.UnregisterHandler<IMixedRealitySpeechHandler>(this); } catch { }
    }

    private void Start() { CoreServices.SpatialAwarenessSystem.Disable(); }

    private void Update() {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.M)) {
            toggleMenu();
        }
#endif
        if (menu.activeSelf) {
            menu.transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));
            menu.transform.forward *= -1;
            if (follow)
                menu.transform.position = player.position + new Vector3(player.forward.x, player.forward.y, player.forward.z).normalized * spawnDistance;
        }
    }

    private void toggleMenu() {
        menu.SetActive(!menu.activeSelf);
        menu.transform.position = player.position + new Vector3(player.forward.x, player.forward.y, player.forward.z).normalized * spawnDistance;
    }

    public void toggleSpatialAwareness() {
        if (toggle.isOn) CoreServices.SpatialAwarenessSystem.Enable();
        else CoreServices.SpatialAwarenessSystem.Disable();
    }

    public void toggleMenuFollow() { follow = !follow; }

    public void toggleNavigation() {
        if (navigating) {
            CoreServices.SpatialAwarenessSystem.Disable();
            instructionText.SetActive(false);
            toggle.isOn = false;
            navigating = false;
        } else {
            CoreServices.SpatialAwarenessSystem.Enable();
            instructionText.SetActive(true);
            toggle.isOn = true;
            navigating = true;
        }
    }

    public void exitApp() { Application.Quit(); }

    void IMixedRealitySpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData) {
        if (eventData.Command.Keyword.ToLower() == "menu") toggleMenu();
        else if (eventData.Command.Keyword.ToLower() == "toggle navigation") toggleNavigation();
    }
}
