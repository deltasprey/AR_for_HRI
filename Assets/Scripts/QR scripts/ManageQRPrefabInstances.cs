using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using QRTracking;

public class ManageQRPrefabInstances : MonoBehaviour, IMixedRealitySpeechHandler {
    public QRCodesManager manager;
    public QRCodesVisualizer visualizer;
    
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
        GameObject qrCodePrefab = visualizer.qrCodePrefab;
        Instantiate(qrCodePrefab, new Vector3(0, 0, 1), Quaternion.identity);
    }

    private void clearMarkers() {
        manager.StopQRTracking();
        QRCode[] qrCodes = FindObjectsOfType<QRCode>();
        foreach (QRCode qrPrefab in qrCodes) {
            Destroy(qrPrefab.gameObject);
        }
        manager.StartQRTracking();
    }

    void IMixedRealitySpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData) {
        if (eventData.Command.Keyword.ToLower() == "clear markers") {
            clearMarkers();
        }
    }
}
