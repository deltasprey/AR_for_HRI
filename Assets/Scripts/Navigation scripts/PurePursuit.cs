using System.Collections;
using UnityEngine;

public class PurePursuit : MonoBehaviour {
    public bool navigating { get; private set; }
    public float forward { get; private set; } = 0f;
    public float turn { get; private set; } = 0f;

    private LocMarkerManager markerManager;
    private TapToPlaceControllerEye navMarkers;
    private Coroutine driveCo;
    private Vector2 robotPos, targetPos2D = Vector2.zero;
    private float robotTheta = 0, targetTheta = 0, angDiff = 0, turnRate = 0;
    private bool follow = false;

    private void Start() { navMarkers = GetComponent<TapToPlaceControllerEye>(); }

    private void OnEnable() {
        markerManager = FindObjectOfType<LocMarkerManager>();
        SelfInteract.navigateToMe += navigate;
        SelfInteract.pathToMe += path;
        SpeechManager.stop += stopCmds;
        SpeechManager.AddListener("Follow Me", followMe);
    }

    private void OnDisable() {
        SelfInteract.navigateToMe -= navigate;
        SelfInteract.pathToMe -= path;
        SpeechManager.stop -= stopCmds;
        SpeechManager.RemoveListener("Follow Me", followMe);
    }

    private void Update() {
        if (navigating) {
            robotPos = new(markerManager.localiser.position.x, markerManager.localiser.position.z);
            robotTheta = markerManager.localiser.eulerAngles.y > 180 ? markerManager.localiser.eulerAngles.y - 360 : markerManager.localiser.eulerAngles.y;
        }
        if (follow) {
            targetPos2D = new(Camera.main.transform.position.x, Camera.main.transform.position.z);
            targetTheta = Mathf.Atan2(targetPos2D.x - robotPos.x, targetPos2D.y - robotPos.y) * Mathf.Rad2Deg;
            angDiff = targetTheta - robotTheta;
            angDiff += (angDiff > 180) ? -360 : (angDiff < -180) ? 360 : 0;
            turnRate = angDiff > 0 ? 0.6f : -0.6f;
            if (!navigating) {
                driveCo = StartCoroutine(drive(turnRate, 0, 1));
                navigating = true;
            }
        }
    }

    private void stopCmds() {
        if (driveCo != null) StopCoroutine(driveCo);
        turn = 0;
        forward = 0;
        follow = false;
        navigating = false;   
    }

    private void followMe() { follow = true; }

    private void navigate(SelfInteract marker) { initialisePursuit(marker, false); }
    
    private void path(SelfInteract marker) { initialisePursuit(marker, true); }

    private void initialisePursuit(SelfInteract marker, bool pathing) {
        if (driveCo != null) StopCoroutine(driveCo);

        int stopId = int.Parse(marker.label.text);
        if (pathing) marker = navMarkers.markers[0].GetComponent<SelfInteract>();
        targetPos2D = new(marker.transform.position.x, marker.transform.position.z);
        robotPos = new(markerManager.localiser.position.x, markerManager.localiser.position.z);
        robotTheta = markerManager.localiser.eulerAngles.y > 180 ? markerManager.localiser.eulerAngles.y - 360 : markerManager.localiser.eulerAngles.y;
        targetTheta = Mathf.Atan2(targetPos2D.x - robotPos.x, targetPos2D.y - robotPos.y) * Mathf.Rad2Deg;
        angDiff = targetTheta - robotTheta;
        angDiff += (angDiff > 180) ? -360 : (angDiff < -180) ? 360 : 0;
        turnRate = angDiff > 0 ? 0.6f : -0.6f;
        navigating = true;
        driveCo = StartCoroutine(drive(turnRate, pathing ? 0 : stopId - 1 , stopId)); // Start driving
        //print($"Target theta = {targetTheta}, Robot theta = {robotTheta}, Ang diff = {angDiff}, Turn rate = {turnRate}");
    }

    IEnumerator drive(float turnRate, int startId, int stopId) {
        //print($"Start ID = {startId}, Stop ID = {stopId}");
        for (int i = startId; i < stopId; i++) {
            // Perform inital turn
            while (Mathf.Abs(targetTheta - robotTheta) > 6) {
                turn = turnRate;
                yield return new WaitForSeconds(0.1f);
            }
            turn = 0;

            // Drive to location
            while ((targetPos2D - robotPos).magnitude > 0.15) {
                turn = (targetTheta - robotTheta) / 50;
                forward = 1;
                yield return new WaitForSeconds(0.1f);
            }
            turn = 0;
            forward = 0;

            // Switch to next waypoint
            if (i < stopId - 1) {
                targetPos2D = new(navMarkers.markers[i + 1].transform.position.z, navMarkers.markers[i + 1].transform.position.x);
                targetTheta = Mathf.Atan2(targetPos2D.x - robotPos.x, targetPos2D.y - robotPos.y) * Mathf.Rad2Deg;
                angDiff = targetTheta - robotTheta;
                angDiff += (angDiff > 180) ? -360 : (angDiff < -180) ? 360 : 0;
                turnRate = angDiff > 0 ? 0.6f : -0.6f;
            }
        }
        navigating = false;
        yield return null;
    }
}