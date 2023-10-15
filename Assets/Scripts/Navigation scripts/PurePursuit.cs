using System.Collections;
using UnityEngine;

public class PurePursuit : MonoBehaviour {
    public bool navigating { get; private set; }
    public float forward { get; private set; } = 0f;
    public float turn { get; private set; } = 0f;

    private LocMarkerManager markerManager;
    private TapToPlaceControllerEye navMarkers;
    private Vector2 rosPos, rosTargetPos2D = Vector2.zero;
    private float rosTheta, targetTheta, turnRate;
    private bool follow = false;

    private void Start() {
        markerManager = FindObjectOfType<LocMarkerManager>();
        navMarkers = GetComponent<TapToPlaceControllerEye>();
    }

    private void OnEnable() {
        SelfInteract.navigateToMe += navigate;
        SelfInteract.pathToMe += path;
        CmdVelControl.msgValueChanged += rosUpdate;
        SpeechManager.stop += stopCmds;
        SpeechManager.AddListener("Follow Me", followMe);
    }

    private void OnDisable() {
        SelfInteract.navigateToMe -= navigate;
        SelfInteract.pathToMe -= path;
        CmdVelControl.msgValueChanged -= rosUpdate;
        SpeechManager.stop -= stopCmds;
        SpeechManager.RemoveListener("Follow Me", followMe);
    }

    private void Update() {
        if (follow) {
            rosTargetPos2D = new(Camera.main.transform.position.z, Camera.main.transform.position.x);
            targetTheta = Mathf.Atan2(rosTargetPos2D.y - rosPos.y, rosTargetPos2D.x - rosPos.x) * Mathf.Rad2Deg;
            turnRate = (targetTheta - rosTheta) > 0 ? 0.6f : -0.6f;
            if (!navigating) {
                StartCoroutine(drive(turnRate, 0, 1));
                navigating = true;
            }
        }
    }

    private void rosUpdate(float x, float z, float theta) {
        rosPos = new Vector2(z, x);
        rosTheta = theta;
        rosTheta %= 360;
        rosTheta = rosTheta > 180 ? rosTheta - 360 : rosTheta;
        targetTheta = Mathf.Atan2(rosTargetPos2D.y - rosPos.y, rosTargetPos2D.x - rosPos.x) * Mathf.Rad2Deg;
    }

    private void stopCmds() {
        StopAllCoroutines();
        turn = 0;
        forward = 0;
        follow = false;
        navigating = false;   
    }

    private void followMe() {
        follow = true;
    }

    private void navigate(SelfInteract marker) { initialisePursuit(marker, false); }
    
    private void path(SelfInteract marker) { initialisePursuit(navMarkers.markers[0].GetComponent<SelfInteract>(), true); }

    private void initialisePursuit(SelfInteract marker, bool pathing) {
        StopAllCoroutines();
        Vector3 rosTargetPos3D = markerManager.rotationMatrix.inverse.MultiplyPoint(new Vector3(marker.transform.position.x, 0, marker.transform.position.z) - markerManager.offset);
        rosTargetPos2D = new(rosTargetPos3D.z, rosTargetPos3D.x);
        //print($"rosPos = {rosPos} | rosTargetPos = {rosTargetPos2D} | Robot theta = {rosTheta}");

        targetTheta = Mathf.Atan2(rosTargetPos2D.y - rosPos.y, rosTargetPos2D.x - rosPos.x) * Mathf.Rad2Deg;
        //print($"Target theta = {targetTheta}");

        turnRate = (targetTheta - rosTheta) > 0 ? 0.6f : -0.6f;
        navigating = true;

        // Start driving
        int stopId = int.Parse(marker.label.text);
        StartCoroutine(drive(turnRate, pathing ? 0 : stopId - 1 , stopId));
    }
    
    IEnumerator drive(float turnRate, int startId, int stopId) {
        for (int i = startId; i < stopId; i++) {
            // Perform inital turn
            while (Mathf.Abs(targetTheta - rosTheta) > 3) {
                turn = turnRate;
                yield return new WaitForSeconds(0.1f);
            }
            turn = 0;

            // Drive to location
            while ((rosTargetPos2D - rosPos).magnitude > 0.1) {
                turn = (targetTheta - rosTheta) / 50;
                forward = 1;
                yield return new WaitForSeconds(0.1f);
            }
            turn = 0;
            forward = 0;

            // Switch to next waypoint
            if (i < stopId - 1) {
                rosTargetPos2D = new(navMarkers.markers[i].transform.position.z, navMarkers.markers[i].transform.position.x);
                targetTheta = Mathf.Atan2(rosTargetPos2D.y - rosPos.y, rosTargetPos2D.x - rosPos.x) * Mathf.Rad2Deg;
                turnRate = (targetTheta - rosTheta) > 0 ? 0.6f : -0.6f;
            }
        }

        navigating = false;
        yield return null;
    }
}