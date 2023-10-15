using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RadialIndicator : MonoBehaviour, IMixedRealityPointerHandler {
    [SerializeField] private float indicatorTimer = 1.0f;
    [SerializeField] private float maxIndicatorTimer = 1.0f;
    [SerializeField] private Image indicator;
    [SerializeField] private KeyCode selectKey = KeyCode.Mouse0;
    [SerializeField] private UnityEvent onClick, onRelease, longClick;

    private bool shouldUpdate = false, loopCompleted = false, keyPressed = false;

    private void Start() { CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this); }

    private void Update() {
        if (!loopCompleted) {
            if (keyPressed || Input.GetKey(selectKey)) {
                // Update radial indicator progress in forward direction
                if (!indicator.enabled)
                    onClick.Invoke();
                shouldUpdate = false;
                indicatorTimer += Time.deltaTime;
                indicator.enabled = true;
                indicator.fillAmount = indicatorTimer;

                // Reset indicator if it's completely filled
                if (indicatorTimer >= maxIndicatorTimer) {
                    indicatorTimer = 0;
                    indicator.fillAmount = 0;
                    indicator.enabled = false;
                    loopCompleted = true; // Stop indicator progress from looping

                    // Invoke long click event
                    longClick.Invoke();
                }
            } else if (shouldUpdate) {
                // Reverse radial indicator direction
                indicatorTimer -= Time.deltaTime;
                indicator.fillAmount = indicatorTimer;

                // Reset indicator if it's empty
                if (indicatorTimer <= 0) {
                    indicatorTimer = 0;
                    indicator.fillAmount = 0;
                    indicator.enabled = false;
                    shouldUpdate = false;
                }
            }
        }

        if (Input.GetKeyUp(selectKey)) {
            shouldUpdate = true;
            loopCompleted = false;
            onRelease.Invoke();
        }
    }

    // Start radial indicator on pinch gesture
    public void OnPointerDown(MixedRealityPointerEventData eventData) { keyPressed = true; }

    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }

    // Pinch gesture released event
    public void OnPointerUp(MixedRealityPointerEventData eventData) {
        shouldUpdate = true;
        loopCompleted = false;
        keyPressed = false;
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
}
