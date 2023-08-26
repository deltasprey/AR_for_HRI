﻿using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Timeline;

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
    private LineRenderer lineRenderer;
    private TextMeshPro _instructionTextMesh;
    private List<GameObject> markers = new();
    private GameObject lookTarget;
    private string _lookAtSurfaceText;
    private bool place = true;
    private int focused = 0, count = 0;

    private void Start() {
        EyeGazeProvider = CoreServices.InputSystem.EyeGazeProvider;
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityFocusHandler>(this);

        lineRenderer = GetComponent<LineRenderer>();

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
                    GameObject marker = Instantiate(_objectToPlace, foundPosition.Value + Vector3.up * 0.05f, Quaternion.Euler(180, 0, 0), _container.transform);
                    marker.GetComponentInChildren<TMP_Text>().text = (++count).ToString();
                    markers.Add(marker);
                    lineRenderer.positionCount = count;
                    lineRenderer.SetPosition(count - 1, foundPosition.Value + Vector3.up * 0.11f);
                }
            } else if (lookTarget != null) {
                int idx = int.Parse(lookTarget.GetComponentInChildren<TMP_Text>().text) - 1;
                markers.RemoveAt(idx);
                for (int i = idx; i < markers.Count; i++) {
                    TMP_Text markerText = markers[i].GetComponentInChildren<TMP_Text>();
                    markerText.text = (int.Parse(markerText.text) - 1).ToString();
                    lineRenderer.SetPosition(i, lineRenderer.GetPosition(i + 1));
                }
                Destroy(lookTarget);
                place = true;
                count--;
                lineRenderer.positionCount = count;
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
