using UnityEngine;
using QRTracking;

public class ManageQRPrefabInstances : MonoBehaviour {
    [SerializeField] private bool spawnOnLoad = false, spawnRotated = false;

    private QRCodesManager manager;
    private QRCodesVisualizer visualizer;
    
    private void OnEnable() {
        manager = GetComponent<QRCodesManager>();
        visualizer = GetComponent<QRCodesVisualizer>();
        SpeechManager.AddListener("clear markers", clearMarkers, true);
        manager.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;

#if UNITY_EDITOR
        if (spawnOnLoad) {
            Invoke(nameof(spawnMarker), 5f);
        }
#endif
    }

    private void OnDisable() {
        SpeechManager.RemoveListener("clear markers", clearMarkers);
        manager.QRCodesTrackingStateChanged -= Instance_QRCodesTrackingStateChanged;
    }

#if UNITY_EDITOR
    private void Update() {
        if (Input.GetKeyDown(KeyCode.K)) {
            QRCode[] qrCodes = FindObjectsOfType<QRCode>();
            foreach (QRCode qrPrefab in qrCodes) {
                visualizer.markerManuallyDespawned(qrPrefab.transform);
                Destroy(qrPrefab.gameObject);
            }
            clearMarkers();
        } else if (Input.GetKeyDown(KeyCode.Backslash)) spawnMarker();
    }

    private void spawnMarker() {
        GameObject qrCodePrefab = visualizer.qrCodePrefab, marker;
        if (spawnRotated) marker = Instantiate(qrCodePrefab, new Vector3(0, 0, 1), Quaternion.Euler(200, 45, 0));
        else marker = Instantiate(qrCodePrefab, new Vector3(0, 0, 1), Quaternion.identity);
        visualizer.markerManuallySpawned(marker.transform.Find("Local Marker").transform);
    }
#endif

    private void OnApplicationQuit() { clearMarkers(); }

    public void InvokeClearMarkers() { clearMarkers(); }

    private void clearMarkers() {
        print("Clearing QR Markers");
        visualizer.enabled = false;
        manager.StopQRTracking();
    }

    private void Instance_QRCodesTrackingStateChanged(object sender, bool status) {
        if (!status) {
            print("Restarting QR Tracking");
            Invoke(nameof(restartQR), 1);
        }
    }

    private void restartQR() {
        visualizer.enabled = true;
        manager.StartQRTracking();
    }
}