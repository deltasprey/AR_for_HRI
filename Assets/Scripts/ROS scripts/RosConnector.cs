/*
© Siemens AG, 2017
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

<http://www.apache.org/licenses/LICENSE-2.0>.

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RosSharp.RosBridgeClient {
    public class RosConnector : MonoBehaviour {
        public int timeout = 10;

        public RosSocket RosSocket { get; private set; }
        public enum Protocols { WebSocketSharp, WebSocketNET, WebSocketUWP };
        public Protocols Protocol;
        public string RosBridgeServerUrl = "ws://192.168.0.1:9090";
        public bool Connected { get; private set; } = false;
        [SerializeField] private MonoBehaviour[] dependentScripts;

        [Header("UI References")]
        [SerializeField] private TMP_InputField serverIP;
        [SerializeField] private TMP_InputField serverPort;
        [SerializeField] private TMP_Text msgText, btnText;
        [SerializeField] private Button connBtn;

        private ManualResetEvent IsConnected = new(false);
        private bool disconnected = false;

        public void Awake() {
            serverIP.text = RosBridgeServerUrl[5..RosBridgeServerUrl.LastIndexOf(":")];
            ConnectToServer();
        }

        public void manualConnect() {
            RosBridgeServerUrl = $"ws://{serverIP.text}:{serverPort.text}";
            Invoke(nameof(ConnectToServer), 0.5f);
            print("Manual Connect");
        }

        private void ConnectToServer() {
            RosBridgeClient.Protocols.IProtocol protocol = GetProtocol();
            protocol.OnConnected += OnConnected;
            protocol.OnClosed += OnClosed;
            RosSocket = new RosSocket(protocol);

            if (!IsConnected.WaitOne(timeout * 1000)) {
                btnText.text = "Connect";
                connBtn.interactable = true;
                serverIP.interactable = true;
                serverPort.interactable = true;
                msgText.text = "Failed to connect to RosBridge at: " + RosBridgeServerUrl;
                msgText.color = new Color(255, 128, 0);
                Debug.LogWarning(msgText.text);
                RosSocket.Close();
            } else {
                foreach (MonoBehaviour script in dependentScripts) {
                    script.enabled = true;
                }
                btnText.text = "Connected";
                connBtn.interactable = false;
                serverIP.interactable = false;
                serverPort.interactable = false;
                msgText.text = "Connected to RosBridge: " + RosBridgeServerUrl;
                msgText.color = Color.green;
            }
        }

        private RosBridgeClient.Protocols.IProtocol GetProtocol() {
#if WINDOWS_UWP
                return new RosBridgeClient.Protocols.WebSocketUWPProtocol(RosBridgeServerUrl);
#else
            switch (Protocol) {
                case Protocols.WebSocketSharp:
                    return new RosBridgeClient.Protocols.WebSocketSharpProtocol(RosBridgeServerUrl);
                case Protocols.WebSocketUWP:
                    Debug.Log("WebSocketUWP only works when deployed to HoloLens, defaulting to WebSocketNetProtocol");
                    return new RosBridgeClient.Protocols.WebSocketNetProtocol(RosBridgeServerUrl);
                default:
                    return new RosBridgeClient.Protocols.WebSocketNetProtocol(RosBridgeServerUrl);
            }
#endif
        }

        private void OnApplicationQuit() {
            RosSocket.Close();
        }

        private void OnConnected(object sender, EventArgs e) {
            IsConnected.Set();
            Connected = true;
            Debug.Log("Connected to RosBridge: " + RosBridgeServerUrl);
        }

        private void OnClosed(object sender, EventArgs e) {
            Connected = false;
            disconnected = true;
            Debug.Log("Disconnected from RosBridge: " + RosBridgeServerUrl);
        }

        private void Update() {
            if (disconnected) {
                btnText.text = "Connect";
                connBtn.interactable = true;
                serverIP.interactable = true;
                serverPort.interactable = true;
                msgText.text = "Disconnected from RosBridge: " + RosBridgeServerUrl;
                msgText.color = new Color(255, 128, 0);
                disconnected = false;
            }
        }
    }
}