using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using QRTracking;
using System.Collections;

public class ManageQRPrefabInstances : MonoBehaviour, IMixedRealitySpeechHandler {
    private QRCodesManager manager;
    private QRCodesVisualizer visualizer;
    
    private void OnEnable() {
        manager = GetComponent<QRCodesManager>();
        visualizer = GetComponent<QRCodesVisualizer>();
        CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this);
        manager.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;
    }

    private void OnDisable() {
        try { CoreServices.InputSystem.UnregisterHandler<IMixedRealitySpeechHandler>(this); } catch { }
        manager.QRCodesTrackingStateChanged -= Instance_QRCodesTrackingStateChanged;
    }

#if UNITY_EDITOR
    private void Update() {
        if (Input.GetKeyDown(KeyCode.K)) {
            QRCode[] qrCodes = FindObjectsOfType<QRCode>();
            foreach (QRCode qrPrefab in qrCodes) Destroy(qrPrefab.gameObject);
            clearMarkers();
        } else if (Input.GetKeyDown(KeyCode.Backslash)) spawnMarker();
    }

    private void spawnMarker() {
        GameObject qrCodePrefab = visualizer.qrCodePrefab;
        Instantiate(qrCodePrefab, new Vector3(0, 0, 1), Quaternion.Euler(200, 45, 0));
    }
#endif

    private void OnApplicationQuit() {
        clearMarkers();
    }

    public void InvokeClearMarkers() {
        clearMarkers();
    }

    private void clearMarkers() {
        print("Clearing QR Markers");
        visualizer.enabled = false;
        manager.StopQRTracking();
    }

    private void Instance_QRCodesTrackingStateChanged(object sender, bool status) {
        if (!status) {
            print("Restarting QR Tracking");
            StartCoroutine(restartQR());
        }
    }

    void IMixedRealitySpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData) {
        if (eventData.Command.Keyword.ToLower() == "clear markers") clearMarkers();
    }

    IEnumerator restartQR() {
        yield return new WaitForSeconds(1);
        visualizer.enabled = true;
        manager.StartQRTracking();
        yield return null;
    }
}