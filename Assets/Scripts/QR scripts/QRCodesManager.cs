﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.QR;

namespace QRTracking {
    public static class QRCodeEventArgs {
        public static QRCodeEventArgs<TData> Create<TData>(TData data) {
            return new QRCodeEventArgs<TData>(data);
        }
    }

    [Serializable]
    public class QRCodeEventArgs<TData> : EventArgs {
        public TData Data { get; private set; }

        public QRCodeEventArgs(TData data) {
            Data = data;
        }
    }

    public class QRCodesManager : Singleton<QRCodesManager> {
        [Tooltip("Determines if the QR codes scanner should be automatically started.")]
        public bool AutoStartQRTracking = true;
        public bool IsTrackerRunning { get; private set; }
        public bool IsSupported { get; private set; }

        public event EventHandler<bool> QRCodesTrackingStateChanged;
        public event EventHandler<QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode>> QRCodeAdded;
        public event EventHandler<QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode>> QRCodeUpdated;
        public event EventHandler<QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode>> QRCodeRemoved;

        private SortedDictionary<Guid, Microsoft.MixedReality.QR.QRCode> qrCodesList = new();

        private QRCodeWatcher qrTracker;
        private bool capabilityInitialized = false;
        private QRCodeWatcherAccessStatus accessStatus;
        private System.Threading.Tasks.Task<QRCodeWatcherAccessStatus> capabilityTask;
        private DateTime startupTime;

        public System.Guid GetIdForQRCode(string qrCodeData) {
            lock (qrCodesList) {
                foreach (var ite in qrCodesList) {
                    if (ite.Value.Data == qrCodeData) {
                        return ite.Key;
                    }
                }
            }
            return new Guid();
        }

        public IList<Microsoft.MixedReality.QR.QRCode> GetList() {
            lock (qrCodesList) {
                return new List<Microsoft.MixedReality.QR.QRCode>(qrCodesList.Values);
            }
        }

        // Use this for initialization
        async protected virtual void Start() {
            startupTime = DateTime.Now;
            IsSupported = QRCodeWatcher.IsSupported();
            capabilityTask = QRCodeWatcher.RequestAccessAsync();
            accessStatus = await capabilityTask;
            capabilityInitialized = true;
        }

        private void SetupQRTracking() {
            try {
                qrTracker = new QRCodeWatcher();
                IsTrackerRunning = false;
                qrTracker.Added += QRCodeWatcher_Added;
                qrTracker.Updated += QRCodeWatcher_Updated;
                qrTracker.Removed += QRCodeWatcher_Removed;
                qrTracker.EnumerationCompleted += QRCodeWatcher_EnumerationCompleted;
            } catch (Exception ex) {
                Debug.Log("QRCodesManager : exception starting the tracker " + ex.ToString());
            }

            if (AutoStartQRTracking) {
                StartQRTracking();
            }
        }

        public void StartQRTracking() {
            if (qrTracker != null && !IsTrackerRunning) {
                Debug.Log("QRCodesManager starting QRCodeWatcher");
                try {
                    qrTracker.Start();
                    IsTrackerRunning = true;
                    QRCodesTrackingStateChanged?.Invoke(this, true);
                } catch (Exception ex) {
                    Debug.Log("QRCodesManager starting QRCodeWatcher Exception:" + ex.ToString());
                }
            }
        }

        public void StopQRTracking() {
            if (IsTrackerRunning) {
                IsTrackerRunning = false;
                if (qrTracker != null) {
                    qrTracker.Stop();
                    qrCodesList.Clear();
                }

                QRCodesTrackingStateChanged?.Invoke(this, false);
            }
        }

        private void QRCodeWatcher_Removed(object sender, QRCodeRemovedEventArgs args) {
            Debug.Log("QRCodesManager QRCodeWatcher_Removed");
            bool found = false;
            lock (qrCodesList) {
                if (qrCodesList.ContainsKey(args.Code.Id)) {
                    qrCodesList.Remove(args.Code.Id);
                    found = true;
                }
            }
            if (found) {
                QRCodeRemoved?.Invoke(this, QRCodeEventArgs.Create(args.Code));
            }
        }

        private void QRCodeWatcher_Updated(object sender, QRCodeUpdatedEventArgs args) {
            //Debug.Log("Time since startup (s):" + (DateTime.Now - startupTime).TotalSeconds);
            //Debug.Log("Last Detected Time:" + args.Code.LastDetectedTime.ToString());
            //Debug.Log($"Time since last detected {(DateTime.Now - args.Code.LastDetectedTime).TotalSeconds} seconds");
            if ((DateTime.Now - args.Code.LastDetectedTime).TotalSeconds < (DateTime.Now - startupTime).TotalSeconds) {
                Debug.Log("QRCodesManager QRCodeWatcher_Updated");
                bool found = false;
                lock (qrCodesList) {
                    if (qrCodesList.ContainsKey(args.Code.Id)) {
                        found = true;
                        qrCodesList[args.Code.Id] = args.Code;
                    }
                }
                if (found) {
                    QRCodeUpdated?.Invoke(this, QRCodeEventArgs.Create(args.Code));
                }
            }
        }

        private void QRCodeWatcher_Added(object sender, QRCodeAddedEventArgs args) {
            Debug.Log("QRCodesManager QRCodeWatcher_Added");
            lock (qrCodesList) {
                qrCodesList[args.Code.Id] = args.Code;
            }

            if ((DateTime.Now - args.Code.LastDetectedTime).TotalSeconds < (DateTime.Now - startupTime).TotalSeconds) {
                var handlers = QRCodeAdded;
                if (handlers != null) {
                    Debug.Log("QRCodesManager QRCodeWatcher_Added_EventFired");
                    handlers(this, QRCodeEventArgs.Create(args.Code));
                }
            }
        }

        private void QRCodeWatcher_EnumerationCompleted(object sender, object e) { Debug.Log("QRCodesManager QrTracker_EnumerationCompleted"); }

        private void Update() {
            if (qrTracker == null && capabilityInitialized && IsSupported) {
                if (accessStatus == QRCodeWatcherAccessStatus.Allowed) {
                    SetupQRTracking();
                } else {
                    Debug.Log("Capability access status : " + accessStatus);
                }
            }
        }
    }
}