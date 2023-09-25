using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using TMPro;
using UnityEngine;

public class JoystickControl : MonoBehaviour {
    public Transform Base, Top;
    public bool isGrabbed { get; private set; } = false;
    public bool stopped { private get; set; } = true;
    public Vector3 rotation { get; private set; }

    [SerializeField] private float deadZone = 0.05f;
    [SerializeField] private TMP_Text forward, right;
    [SerializeField] private GameObject stop, go;
    [SerializeField] private bool attachToHand = false;
    private bool tracking = false;
    private Vector3 topPos, topRot;
    private MixedRealityPose pose;
    private float x, y;

    private void Start() {
        // Store (initial) central position
        topPos = new(Top.localPosition.x, Top.localPosition.y, Top.localPosition.z);
        topRot = new(Top.localEulerAngles.x, Top.localEulerAngles.y, Top.localEulerAngles.z);
    }

    private void Update() {
        Base.LookAt(Top);
        Base.localEulerAngles = new Vector3(Base.localEulerAngles.x, Base.localEulerAngles.y, 0);
        transform.localEulerAngles = new Vector3(-110, Camera.main.transform.rotation.eulerAngles.y, 0);

        if (isGrabbed) {
            // Update public var with joystick's rotation when its rotation exceeds the deadzone
            x = Mathf.Repeat(Base.localEulerAngles.x + 180, 360) - 180;
            y = Mathf.Repeat(Base.localEulerAngles.y + 180, 360) - 180;

            if (Mathf.Abs(x) > deadZone) {
                x /= 50;
            } else {
                x = 0;
            }
            if (Mathf.Abs(y) > deadZone) {
                y /= 50;
            } else {
                y = 0;
            }
            rotation = new(x, y);
        } else {
            // Rotate the joystick back to its central position
            rotation = Vector3.zero;
            Top.localPosition = Vector3.MoveTowards(Top.localPosition, topPos, 0.01f);
            Top.localEulerAngles = Vector3.RotateTowards(Top.localEulerAngles, topRot, 0.1f, 1f);
        }
        forward.text = "Forward\n" + rotation.x.ToString("F3");
        right.text = "Right\n" + rotation.y.ToString("F3");

        if (attachToHand) {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Left, out pose)) {
                if (!tracking) {
                    StopAllCoroutines();
                    transform.position = pose.Position;
                    tracking = true;
                    StartCoroutine(trackHand());
                }
            } else {
                if (tracking) {
                    tracking = false;
                }
            }
        }

        if (stopped) {
            if (go.activeSelf) {
                stop.SetActive(true);
                go.SetActive(false);
            }
        } else {
            if (stop.activeSelf) {
                stop.SetActive(false);
                go.SetActive(true);
            }
        }
    }

    public void grabbed() {
        isGrabbed = true;
    }

    public void released() {
        isGrabbed = false;
    }

    IEnumerator trackHand() {
        while (tracking) {
            transform.position = Vector3.MoveTowards(transform.position, 
                                                     pose.Position,
                                                     Mathf.Max(Vector3.Magnitude(transform.position - pose.Position)/6, 0.01f));
            yield return new WaitForSeconds(0.005f);
        }

        // Delay if hand tracking lost
        while (isGrabbed) yield return new WaitForSeconds(0.2f);
        yield return new WaitForSeconds(2);
        if (tracking) yield return null;
        transform.localPosition = new Vector3(2, 0, 3);
        yield return null;
    }
}
