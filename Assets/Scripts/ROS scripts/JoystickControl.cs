using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class JoystickControl : MonoBehaviour {
    public Transform Base, Top;
    public bool isGrabbed { get; private set; } = false;
    public Vector3 rotation { get; private set; }
    public float deadZone = 0.05f;

    [SerializeField] private bool attachToHand = false;
    private Vector3 topPos, topRot;
    private MixedRealityPose pose;
    private float x, y;

    private void Start() {
        // Store (initial) central position
        topPos = new(Top.localPosition.x, Top.localPosition.y, Top.localPosition.z);
        topRot = new(Top.localEulerAngles.x, Top.localEulerAngles.y, Top.localEulerAngles.z);
    }

    private void Update() {
        if (isGrabbed) {
            // Update public var with joystick's rotation when its rotation exceeds the deadzone
            x = 0; y = 0;
            if (Mathf.Abs(Base.localRotation.x) > deadZone) {
                x = Base.localRotation.x * 2;
            }
            if (Mathf.Abs(Base.localRotation.y) > deadZone) {
                y = Base.localRotation.y * 2;
            }
            rotation = new(x, y, 0);
        } else {
            // Rotate the joystick back to its central position
            rotation = Vector3.zero;
            Top.localPosition = Vector3.MoveTowards(Top.localPosition, topPos, 0.01f);
            Top.localEulerAngles = Vector3.RotateTowards(Top.localEulerAngles, topRot, 0.1f, 1f);
        }

        if (attachToHand) {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Left, out pose)) {
                Base.parent.SetPositionAndRotation(pose.Position, pose.Rotation * Quaternion.Euler(-90, -90, 0));
            } else {
                Base.parent.localPosition = new Vector3(2, 0, 3);
                Base.parent.localEulerAngles = new Vector3(-90, 0);
            }
        }
    }

    public void grabbed() {
        isGrabbed = true;
    }

    public void released() {
        isGrabbed = false;
    }
}
