using System.Collections;
using UnityEngine;

public class PurePursuit : MonoBehaviour {
    public bool navigating { get; private set; }
    public float forward { get; private set; } = 0f;
    public float turn { get; private set; } = 0f;

    private LocMarkerManager markerManager;
    //private TapToPlaceControllerEye navMarkers;
    private Vector2 rosPos, rosTargetPos2D = Vector2.zero;
    private float rosTheta, targetTheta;

    private void OnEnable() {
        markerManager = FindObjectOfType<LocMarkerManager>();
        //navMarkers = GetComponent<TapToPlaceControllerEye>();
        SelfInteract.navigateToMe += navigate;
        CmdVelControl.msgValueChanged += rosUpdate;
        SafetyZone.stop += stopCmds;
    }

    private void OnDisable() {
        SelfInteract.navigateToMe -= navigate;
        CmdVelControl.msgValueChanged -= rosUpdate;
        SafetyZone.stop -= stopCmds;
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
        navigating = false;
    }

    private void navigate(SelfInteract marker) {
        StopAllCoroutines();
        Vector3 rosTargetPos3D = markerManager.rotationMatrix.inverse.MultiplyPoint(new Vector3(marker.transform.position.x, 0, marker.transform.position.z) - markerManager.offset);
        rosTargetPos2D = new(rosTargetPos3D.z, rosTargetPos3D.x);
        //print($"rosPos = {rosPos} | rosTargetPos = {rosTargetPos2D} | Robot theta = {rosTheta}");

        targetTheta = Mathf.Atan2(rosTargetPos2D.y - rosPos.y, rosTargetPos2D.x - rosPos.x) * Mathf.Rad2Deg;
        //print($"Target theta = {targetTheta}");

        float turnRate = (targetTheta - rosTheta) > 0 ? 0.6f : -0.6f;
        navigating = true;

        // Start driving
        StartCoroutine(drive(turnRate));
    }
    
    IEnumerator drive(float turnRate) {
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
        navigating = false;
        yield return null;
    }
}
