using System.Collections.Generic;
using UnityEngine;

namespace QRTracking {
    public class QRCodesVisualizer : MonoBehaviour {
        public GameObject qrCodePrefab;
        public delegate void QREvent(Transform marker);
        public static event QREvent markerSpawned;
        public static event QREvent markerDespawned;

        private SortedDictionary<System.Guid, GameObject> qrCodesObjectsList;
        private GameObject qrCodeObject;
        private bool clearExisting = false;

        struct ActionData {
            public enum Type {
                Added,
                Updated,
                Removed
            };
            public Type type;
            public Microsoft.MixedReality.QR.QRCode qrCode;

            public ActionData(Type type, Microsoft.MixedReality.QR.QRCode qRCode) : this() {
                this.type = type;
                qrCode = qRCode;
            }
        }

        private Queue<ActionData> pendingActions = new();

        // Use this for initialization
        void Start() {
            Debug.Log("QRCodesVisualizer start");
            qrCodesObjectsList = new SortedDictionary<System.Guid, GameObject>();

            QRCodesManager.Instance.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;
            QRCodesManager.Instance.QRCodeAdded += Instance_QRCodeAdded;
            QRCodesManager.Instance.QRCodeUpdated += Instance_QRCodeUpdated;
            QRCodesManager.Instance.QRCodeRemoved += Instance_QRCodeRemoved;
            if (qrCodePrefab == null) {
                throw new System.Exception("Prefab not assigned");
            }
        }

        private void OnDisable() {
            foreach (var obj in qrCodesObjectsList) {
                Destroy(obj.Value);
                markerDespawned?.Invoke(obj.Value.transform);
            }
            qrCodesObjectsList.Clear();
            pendingActions.Clear();
        }

        private void Instance_QRCodesTrackingStateChanged(object sender, bool status) { if (!status) clearExisting = true; }

        private void Instance_QRCodeAdded(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e) {
            Debug.Log("QRCodesVisualizer Instance_QRCodeAdded");
            lock (pendingActions) { pendingActions.Enqueue(new ActionData(ActionData.Type.Added, e.Data)); }
        }

        private void Instance_QRCodeUpdated(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e) {
            Debug.Log("QRCodesVisualizer Instance_QRCodeUpdated");
            lock (pendingActions) { pendingActions.Enqueue(new ActionData(ActionData.Type.Updated, e.Data)); }
        }

        private void Instance_QRCodeRemoved(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e) {
            Debug.Log("QRCodesVisualizer Instance_QRCodeRemoved");
            lock (pendingActions) { pendingActions.Enqueue(new ActionData(ActionData.Type.Removed, e.Data)); }
        }

        // Update is called once per frame
        void Update() {
            lock (pendingActions) {
                while (pendingActions.Count > 0) {
                    var action = pendingActions.Dequeue();
                    if (action.type == ActionData.Type.Added) {
                        qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        qrCodeObject.GetComponent<SpatialGraphCoordinateSystem>().Id = action.qrCode.SpatialGraphNodeId;
                        qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                        qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                        markerSpawned?.Invoke(qrCodeObject.transform.Find("Local Marker").transform);
                    } else if (action.type == ActionData.Type.Updated) {
                        if (!qrCodesObjectsList.ContainsKey(action.qrCode.Id)) {
                            qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                            qrCodeObject.GetComponent<SpatialGraphCoordinateSystem>().Id = action.qrCode.SpatialGraphNodeId;
                            qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                            qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                            markerSpawned?.Invoke(qrCodeObject.transform.Find("Local Marker").transform);
                        }
                    } else if (action.type == ActionData.Type.Removed) {
                        if (qrCodesObjectsList.ContainsKey(action.qrCode.Id)) {
                            markerDespawned?.Invoke(qrCodesObjectsList[action.qrCode.Id].transform);
                            Destroy(qrCodesObjectsList[action.qrCode.Id]);
                            qrCodesObjectsList.Remove(action.qrCode.Id);
                            print("QR destroyed");
                        }
                    }
                }
            }
            if (clearExisting) {
                clearExisting = false;
                foreach (var obj in qrCodesObjectsList) {
                    Destroy(obj.Value);
                    markerDespawned?.Invoke(obj.Value.transform);
                }
                qrCodesObjectsList.Clear();
            }
        }

        public void markerManuallySpawned(Transform marker) { markerSpawned?.Invoke(marker); }

        public void markerManuallyDespawned(Transform marker) { markerDespawned?.Invoke(marker); }
    }
}