using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using QRTracking;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TapToPlaceControllerEye : MonoBehaviour, IMixedRealityFocusHandler {
    public List<GameObject> markers { get; private set; } = new();

    [SerializeField] private float _maxDistance = 3;
    [SerializeField] private GameObject _instructionText, _objectToPlace, _container, _indicator;
    [SerializeField] private UnityEvent ValidSurface, NoValidSurface;

    private IMixedRealityEyeGazeProvider EyeGazeProvider;
    private LineRenderer lineRenderer;
    private TextMeshPro _instructionTextMesh;
    private GameObject lookTarget;
    private Transform locMarker;
    private string _lookAtSurfaceText;
    private bool place = true, placing = false;
    private int focused = 0, count = 1, ignoreLayerId;
    private Vector3? foundPosition;

    private void Start() {
        ignoreLayerId = LayerMask.NameToLayer("UI");
        EyeGazeProvider = CoreServices.InputSystem.EyeGazeProvider;

        lineRenderer = GetComponent<LineRenderer>();

        _instructionTextMesh = _instructionText.GetComponentInChildren<TextMeshPro>();
        _lookAtSurfaceText = $"Please look at the spatial map max {_maxDistance}m ahead of you";
        _instructionTextMesh.text = _lookAtSurfaceText;

        QRCodesVisualizer.markerSpawned += markerSpawned;
        QRCodesVisualizer.markerDespawned += markerDespawned;
        SelfInteract.removeMe += removeMarker;
        SelfInteract.ignoreMe += ignoreMarker;
    }

    private void OnEnable() { 
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityFocusHandler>(this);
        SpeechManager.AddListener("clear markers", removeAllMarkers, true);
    }

    private void OnDisable() {
        SpeechManager.RemoveListener("clear markers", removeAllMarkers);
        try { CoreServices.InputSystem?.UnregisterHandler<IMixedRealityFocusHandler>(this); } catch { }
    }

    private void Update() {
        if (placing && focused > 0) {
            foundPosition = EyeGazeProvider.HitInfo.point;
            if (foundPosition != null) _indicator.transform.position = foundPosition.Value + Vector3.up * 0.05f;
        }
        if (locMarker != null) lineRenderer.SetPosition(0, locMarker.position);
    }

    private void markerSpawned(Transform marker) { locMarker = marker; }

    private void markerDespawned(Transform _) {
        locMarker = null;
        if (lineRenderer.positionCount > 1) lineRenderer.SetPosition(0, lineRenderer.GetPosition(1));
    }

    public void PlaceRemoveMarker() {
        _indicator.SetActive(false);
        if (_instructionText.activeSelf && focused > 0) {
            if (place) {
                foundPosition = EyeGazeProvider.HitInfo.point;
                if (foundPosition != null) {
                    GameObject marker = Instantiate(_objectToPlace, foundPosition.Value + Vector3.up * 0.05f, Quaternion.Euler(180, 0, 0), _container.transform);
                    marker.GetComponentInChildren<TMP_Text>().text = (count++).ToString();
                    markers.Add(marker);
                    lineRenderer.positionCount = count;
                    lineRenderer.SetPosition(count - 1, foundPosition.Value + Vector3.up * 0.11f);
                    if (count == 2 && locMarker == null) lineRenderer.SetPosition(0, foundPosition.Value + Vector3.up * 0.11f);
                }
            } else if (lookTarget != null) {
                int idx = int.Parse(lookTarget.GetComponentInChildren<TMP_Text>().text) - 1;
                markers.RemoveAt(idx);
                for (int i = idx; i < markers.Count; i++) {
                    TMP_Text markerText = markers[i].GetComponentInChildren<TMP_Text>();
                    markerText.text = (int.Parse(markerText.text) - 1).ToString();
                    lineRenderer.SetPosition(i + 1, lineRenderer.GetPosition(i + 2));
                }
                Destroy(lookTarget);
                place = true;
                count--;
                lineRenderer.positionCount = count;
                if (count > 1 && locMarker == null) lineRenderer.SetPosition(0, lineRenderer.GetPosition(1));
                focused -= 2;
            }
        }
    }

    void IMixedRealityFocusHandler.OnFocusEnter(FocusEventData eventData) {
        focused++;
        if (eventData.NewFocusedObject != null) {
            if (eventData.NewFocusedObject.layer != ignoreLayerId) {
                ValidSurface.Invoke();
                _instructionTextMesh.text = "Tap to select a location";
            } else {
                _indicator.SetActive(false);
                NoValidSurface.Invoke();
                _instructionTextMesh.text = _lookAtSurfaceText;
            }
        }
    }

    void IMixedRealityFocusHandler.OnFocusExit(FocusEventData eventData) {
        if (--focused == 0) {
            _indicator.SetActive(false);
            NoValidSurface.Invoke();
            _instructionTextMesh.text = _lookAtSurfaceText;
        }
    }

    public void removeMarker(SelfInteract marker) {
        place = false;
        placing = false;
        lookTarget = marker.gameObject;
        _indicator.SetActive(false);
    }

    public void ignoreMarker(SelfInteract _) {
        place = true;
        lookTarget = null;
    }

    public void removeAllMarkers() {
        count = 1;
        lineRenderer.positionCount = count;
        foreach (GameObject marker in markers) Destroy(marker);
        markers.Clear();
    }

    public void indicateMarkerLoc() {
        if (place && focused > 0) {
            placing = true;
            _indicator.SetActive(true);
        }
    }

    public void removeIndicator() {
        placing = false;
        _indicator.SetActive(false);
    }
}