using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class SafetyZone : MonoBehaviour {
    public GameObject handCollider, headCollider;
    public Material zoneMat;
    public delegate void StopCmd();
    public static event StopCmd stop;
    public static event StopCmd restart;

    GameObject rHandObj, lHandObj, headObj;
    MixedRealityPose pose;
    Color goColor = Color.green, stopColor = Color.red;
    uint stopped = 0;

    void Start() {
        rHandObj = Instantiate(handCollider);
        lHandObj = Instantiate(handCollider);
        headObj = Instantiate(headCollider);
        restart?.Invoke();
    }

    void Update() {
        headObj.transform.position = Camera.main.transform.position;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleKnuckle, Handedness.Right, out pose)) {
            rHandObj.transform.position = pose.Position;
        }
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleKnuckle, Handedness.Left, out pose)) {
            lHandObj.transform.position = pose.Position;
        }
    }
    
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {          
            stopped++;
            if (stopped == 1) {
                stop?.Invoke();
                zoneMat.SetColor("_Line_Color_", stopColor);
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (stopped > 0 && other.CompareTag("Player")) {
            stopped--;
            if (stopped == 0) {
                restart?.Invoke();
                zoneMat.SetColor("_Line_Color_", goColor);
            }
        }
    }
}
