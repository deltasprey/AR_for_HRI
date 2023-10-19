using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechManager : MonoBehaviour, IMixedRealitySpeechHandler {
    public delegate void SpeechEvent();
    public static event SpeechEvent stop;

    [SerializeField] private GameObject listeningText;
    [SerializeField] private float listeningTimeout = 10f;

    // Create a dictionary to map string keys to SpeechEvent events.
    private static Dictionary<string, SpeechEvent> speechEventDictionary = new();
    private static Dictionary<string, SpeechEvent> speechSafeEventDictionary = new();
    private Coroutine tout;
    private bool listening = false;

    private void OnEnable() { CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this); }

    private void OnDisable() {
        try { CoreServices.InputSystem.UnregisterHandler<IMixedRealitySpeechHandler>(this); } catch { }
    }

    // Subscribe a method to a SpeechEvent with a given key.
    public static void AddListener(string key, SpeechEvent listener, bool safe = false) {
        key = key.ToLower();
        if (safe) {
            if (!speechSafeEventDictionary.ContainsKey(key)) {
                speechSafeEventDictionary[key] = null; // Initialize the event if it doesn't exist.
            }
            speechSafeEventDictionary[key] += listener; // Add the listener to the event.
        } else {
            if (!speechEventDictionary.ContainsKey(key)) {
                speechEventDictionary[key] = null; // Initialize the event if it doesn't exist.
            }
            speechEventDictionary[key] += listener; // Add the listener to the event.
        }
    }

    // Unsubscribe a method from a SpeechEvent with a given key.
    public static void RemoveListener(string key, SpeechEvent listener) {
        key = key.ToLower();
        if (speechEventDictionary.ContainsKey(key)) {
            speechEventDictionary[key] -= listener; // Remove the listener from the event.
        }
        if (speechSafeEventDictionary.ContainsKey(key)) {
            speechSafeEventDictionary[key] -= listener; // Remove the listener from the event.
        }
    }

    // Invoke the SpeechEvent with a given key.
    public static void InvokeEvent(string key, bool listening = false) {
        key = key.ToLower();
        if (key == "stop") {
            stop?.Invoke();
        } if (speechSafeEventDictionary.ContainsKey(key) && speechSafeEventDictionary[key] != null) {
            speechSafeEventDictionary[key]?.Invoke();
        } else if (listening && speechEventDictionary.ContainsKey(key) && speechEventDictionary[key] != null) {
            speechEventDictionary[key]?.Invoke();
        }
    }

    void IMixedRealitySpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData) {
        string cmd = eventData.Command.Keyword.ToLower();
        if (cmd == "stop") {
            stop?.Invoke();
        } else if (cmd == "command") {
            if (tout != null) StopCoroutine(tout);
            listening = true;
            listeningText.SetActive(true);
            tout = StartCoroutine(timeout());
        } else {
            InvokeEvent(cmd, listening);
            if (tout != null) StopCoroutine(tout);
            listening = false;
            listeningText.SetActive(false);
        }
    }

    IEnumerator timeout() {
        yield return new WaitForSeconds(listeningTimeout);
        listening = false;
        listeningText.SetActive(false);
        yield return null;
    }
}