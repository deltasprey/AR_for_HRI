using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class HandController : MonoBehaviour {
    public Material zoneMat;

    MixedRealityPose pose;
    uint started = 0;

    private void Update() {
        if (started > 0) {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleKnuckle, Handedness.Right, out pose)) {
                //pose.Rotation
            }
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleKnuckle, Handedness.Left, out pose)) {
                //pose.Rotation
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            started++;
            if (started == 1) {
                zoneMat.SetColor("_Line_Color_", Color.green);
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (started > 0 && other.CompareTag("Player")) {
            started--;
            if (started == 0) {
                zoneMat.SetColor("_Line_Color_", Color.yellow);
            }
        }
    }
}
