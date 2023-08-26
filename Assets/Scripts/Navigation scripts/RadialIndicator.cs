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
    [SerializeField] private UnityEvent myEvent;

    private bool shouldUpdate = false, loopCompleted = false, keyPressed = false;

    private void Start() {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
    }

    private void Update() {
        if (!loopCompleted) {
            if (keyPressed || Input.GetKey(selectKey)) {
                shouldUpdate = false;
                indicatorTimer += Time.deltaTime;
                indicator.enabled = true;
                indicator.fillAmount = indicatorTimer;

                if (indicatorTimer >= maxIndicatorTimer) {
                    indicatorTimer = 0;
                    indicator.fillAmount = 0;
                    indicator.enabled = false;
                    loopCompleted = true;
                    myEvent.Invoke();
                }
            } else if (shouldUpdate) {
                indicatorTimer -= Time.deltaTime;
                indicator.fillAmount = indicatorTimer;

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
        }
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData) {
        keyPressed = true;
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }

    public void OnPointerUp(MixedRealityPointerEventData eventData) {
        shouldUpdate = true;
        loopCompleted = false;
        keyPressed = false;
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
}
