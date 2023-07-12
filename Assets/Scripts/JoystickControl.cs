using UnityEngine;

public class JoystickControl : MonoBehaviour {
    public Transform Base, Top;
    public bool isGrabbed { get; private set; } = false;
    public Vector3 rotation { get; private set; }
    public float deadZone = 0.05f;

    private Vector3 topPos, topRot;
    private float x, y;

    void Start() {
        topPos = new(Top.localPosition.x, Top.localPosition.y, Top.localPosition.z);
        topRot = new(Top.localEulerAngles.x, Top.localEulerAngles.y, Top.localEulerAngles.z);
    }

    void Update() {
        if (isGrabbed) {
            x = 0; y = 0;
            if (Mathf.Abs(Base.localRotation.x) > deadZone) {
                x = Base.localRotation.x * 2;
            }
            if (Mathf.Abs(Base.localRotation.y) > deadZone) {
                y = Base.localRotation.y * 2;
            }
            rotation = new(x, y, 0);
        } else {
            rotation = Vector3.zero;
            Top.localPosition = Vector3.MoveTowards(Top.localPosition, topPos, 0.01f);
            Top.localEulerAngles = Vector3.RotateTowards(Top.localEulerAngles, topRot, 0.1f, 1f);
        }
    }

    public void grabbed() {
        isGrabbed = true;
    }

    public void released() {
        isGrabbed = false;
    }
}
