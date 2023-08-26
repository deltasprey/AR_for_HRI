using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class HandController : MonoBehaviour {
    public Material zoneMat;
    public float maxOffset = 0.05f;
    public bool tracking { get; private set; } = false;
    public Vector3 rotation { get; private set; }

    private MixedRealityPose pose;
    private Quaternion initRot;
    private uint started = 0;

    private void OnDisable() {
        zoneMat.SetColor("_Line_Color_", Color.yellow);
    }

    private void Update() {
        if (started > 0) {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Right, out pose)) {
                IsHandInSphere();
                if (tracking) {
                    TrackHandRotation();
                }
            } else if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Left, out pose)) {
                IsHandInSphere();
                if (tracking) {
                    TrackHandRotation();
                }
            }
        }
    }

    private void IsHandInSphere() {
        if (!tracking && Vector3.Distance(transform.position, pose.Position) <= maxOffset) {
            zoneMat.SetColor("_Line_Color_", Color.green);
            initRot = new Quaternion(pose.Rotation.x, pose.Rotation.y, pose.Rotation.z, pose.Rotation.w);
            tracking = true;
        } else if (tracking && Vector3.Distance(transform.position, pose.Position) > maxOffset) {
            zoneMat.SetColor("_Line_Color_", Color.yellow);
            tracking = false;
        }
    }

    private void TrackHandRotation() {
        rotation = (pose.Rotation * Quaternion.Inverse(initRot)).eulerAngles;
        //alter angles maybe
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            started++;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (started > 0 && other.CompareTag("Player")) {
            started--;
            if (started == 0) {
                zoneMat.SetColor("_Line_Color_", Color.yellow);
                tracking = false;
            }
        }
    }
}
