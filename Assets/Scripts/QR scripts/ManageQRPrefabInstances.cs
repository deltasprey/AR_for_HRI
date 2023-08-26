using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

public class ManageQRPrefabInstances : MonoBehaviour, IMixedRealitySpeechHandler {
    private void OnEnable() {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this);
    }

    private void OnDisable() {
        try {
            CoreServices.InputSystem.UnregisterHandler<IMixedRealitySpeechHandler>(this);
        } catch { }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.K)) {
            clearMarkers();
        } else if (Input.GetKeyDown(KeyCode.Backslash)) {
            spawnMarker();
        }
    }

    private void OnApplicationQuit() {
        clearMarkers();
    }

    private void spawnMarker() {
        GameObject qrCodePrefab = GetComponent<QRTracking.QRCodesVisualizer>().qrCodePrefab;
        Instantiate(qrCodePrefab, new Vector3(0, 0, 1), Quaternion.identity);
    }

    private void clearMarkers() {
        QRTracking.QRCode[] qrCodes = FindObjectsOfType<QRTracking.QRCode>();
        foreach (QRTracking.QRCode qrPrefab in qrCodes) {
            Destroy(qrPrefab.gameObject);
        }
    }

    void IMixedRealitySpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData) {
        if (eventData.Command.Keyword.ToLower() == "clear markers") {
            clearMarkers();
        }
    }
}
