using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using TMPro;
using UnityEngine;

public class TapToPlaceControllerEye : MonoBehaviour, IMixedRealityFocusHandler {
    [SerializeField]
    private GameObject _instructionText;

    [SerializeField]
    private float _maxDistance = 3;

    [SerializeField]
    private GameObject _objectToPlace;

    [SerializeField]
    private GameObject _container;

    private IMixedRealityEyeGazeProvider EyeGazeProvider;
    private TextMeshPro _instructionTextMesh;
    private GameObject lookTarget;
    private string _lookAtSurfaceText;
    private bool place = true;
    private int focused = 0;

    private void Start() {
        EyeGazeProvider = CoreServices.InputSystem.EyeGazeProvider;
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityFocusHandler>(this);

        _instructionTextMesh = _instructionText.GetComponentInChildren<TextMeshPro>();
        _lookAtSurfaceText = $"Please look at the spatial map max {_maxDistance}m ahead of you";
        _instructionTextMesh.text = _lookAtSurfaceText;

        SelfInteract.removeMe += removeMarker;
        SelfInteract.ignoreMe += ignoreMarker;
    }

    private void Update() {
        _instructionTextMesh.text = focused > 0 ? "Tap to select a location" : _lookAtSurfaceText;
    }

    public void PlaceRemoveMarker() {
        if (_instructionText.activeSelf && focused > 0) {
            if (place) {
                Vector3? foundPosition = EyeGazeProvider.HitInfo.point;
                if (foundPosition != null) {
                    Instantiate(_objectToPlace, foundPosition.Value, Quaternion.identity, _container.transform);
                }
            } else if (lookTarget != null) {
                Destroy(lookTarget);
                place = true;
            }
        }
    }

    void IMixedRealityFocusHandler.OnFocusEnter(FocusEventData eventData) {
        focused++;
    }

    void IMixedRealityFocusHandler.OnFocusExit(FocusEventData eventData) {
        focused--;
    }

    public void removeMarker(SelfInteract marker) {
        place = false;
        lookTarget = marker.gameObject;
    }

    public void ignoreMarker(SelfInteract _) {
        place = true;
        lookTarget = null;
    }
}
