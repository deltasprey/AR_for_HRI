using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

public class Fireworks : MonoBehaviour, IMixedRealitySpeechHandler {
    public List<GameObject> fireworks = new();

    private void OnEnable() {
        if (CoreServices.InputSystem != null) {
            CoreServices.InputSystem.RegisterHandler<IMixedRealitySpeechHandler>(this);
        }
    }

    private void OnDisable() {
        try {
            CoreServices.InputSystem.UnregisterHandler<IMixedRealitySpeechHandler>(this);
        } catch {}
    }

    void IMixedRealitySpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData) {
        if (eventData.Command.Keyword.ToLower() == "fireworks") {
            StartCoroutine(explodeFireworks());
        }
    }

    public void lookPress() {
        StartCoroutine(explodeFireworks());
    }

    IEnumerator explodeFireworks() {
        foreach (GameObject firework in fireworks) {
            firework.GetComponent<ParticleSystem>().Play();
            firework.GetComponent<AudioSource>().Play();
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
    }
}
