using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;

public class HandTracking : MonoBehaviour {
    public GameObject sphereMarker;

    private GameObject rThumbObject, rMiddleObject, rRingObject, rPinkyObject;
    private Renderer rThumbRend, rMiddleRend, rRingRend, rPinkyRend;
    private MixedRealityPose pose;

    void Start() {
        // Right hand
        rThumbObject = Instantiate(sphereMarker, this.transform);
        rMiddleObject = Instantiate(sphereMarker, this.transform);
        rRingObject = Instantiate(sphereMarker, this.transform);
        rPinkyObject = Instantiate(sphereMarker, this.transform);
        rThumbRend = rThumbObject.GetComponent<Renderer>();
        rMiddleRend = rMiddleObject.GetComponent<Renderer>();
        rRingRend = rRingObject.GetComponent<Renderer>();
        rPinkyRend = rPinkyObject.GetComponent<Renderer>();
    }

    void Update() {
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Right, out pose)) {
            rThumbRend.enabled = true;
            rThumbObject.transform.position = pose.Position;
        } else {
            rThumbRend.enabled = false;
        }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, Handedness.Right, out pose)) {
            rMiddleRend.enabled = true;
            rMiddleObject.transform.position = pose.Position;
        } else {
            rMiddleRend.enabled = false;
        }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, Handedness.Right, out pose)) {
            rRingRend.enabled = true;
            rRingObject.transform.position = pose.Position;
        } else {
            rRingRend.enabled = false;
        }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyTip, Handedness.Right, out pose)) {
            rPinkyRend.enabled = true;
            rPinkyObject.transform.position = pose.Position;
        } else {
            rPinkyRend.enabled = false;
        }
    }
}
