using System.Collections;
using UnityEngine;

public class PurePursuit : MonoBehaviour {
    public bool navigating { get; private set; }
    public float forward { get; private set; } = 0f;
    public float turn { get; private set; } = 0f;

    private LocMarkerManager markerManager;
    //private TapToPlaceControllerEye navMarkers;
    private Vector2 rosPos;
    private float rosTheta;

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
        rosPos = new Vector2(x, z);
        rosTheta = theta;
        rosTheta %= 360;
        rosTheta = rosTheta > 180 ? rosTheta - 360 : rosTheta;
    }

    private void stopCmds() {
        StopAllCoroutines();
        turn = 0;
        forward = 0;
        navigating = false;
    }

    private void navigate(SelfInteract marker) {
        Vector3 rosTargetPos3D = markerManager.rotationMatrix.inverse.MultiplyPoint3x4(new Vector3(marker.transform.position.x, 0, marker.transform.position.z));
        Vector2 rosTargetPos2D = new(rosTargetPos3D.x, rosTargetPos3D.z);
        float targetTheta = Vector2.SignedAngle(rosPos, rosTargetPos2D) - markerManager.offsetTheta;
        print($"Target theta = {targetTheta}");
        print($"Target theta = {targetTheta}");

        float turnRate = (targetTheta - rosTheta) > 0 ? 0.6f : -0.6f;
        navigating = true;

        // Start driving
        StartCoroutine(drive(targetTheta, turnRate, rosTargetPos2D));
    }
    
    IEnumerator drive(float targetTheta, float turnRate, Vector2 rosTargetPos2D) {
        // Perform inital turn
        while (Mathf.Abs(targetTheta - rosTheta) > 5) {
            turn = turnRate;
            yield return new WaitForSeconds(0.1f);
        }
        turn = 0;

        // Drive to location
        while ((rosTargetPos2D - rosPos).magnitude > 0.1) {
            turn = (targetTheta - rosTheta) / 50;
            forward = 0.5f;
            yield return new WaitForSeconds(0.1f);
        }

        turn = 0;
        forward = 0;
        navigating = false;
        yield return null;
    }
}
