using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class SafetyZone : MonoBehaviour {
    [SerializeField] private GameObject handCollider, headCollider;
    [SerializeField] private Material zoneMat;

    private GameObject rHandObj, lHandObj, headObj;
    private MixedRealityPose pose;
    private uint stopped = 0;

    private void Start() {
        // Instantiate collidable prefab objects on the user's head and hands
        headObj = Instantiate(headCollider, Camera.main.transform);
        rHandObj = Instantiate(handCollider, headObj.transform);
        lHandObj = Instantiate(handCollider, headObj.transform);
        SpeechManager.InvokeEvent("restart", true);
    }

    private void OnDestroy() {
        Destroy(rHandObj);
        Destroy(lHandObj);
        Destroy(headObj);
    }

    private void Update() {
        // Update prefab's positions to user's head and hands
        headObj.transform.rotation = Quaternion.identity;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleKnuckle, Handedness.Right, out pose)) rHandObj.transform.position = pose.Position;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleKnuckle, Handedness.Left, out pose)) lHandObj.transform.position = pose.Position;
    }

    private void OnTriggerEnter(Collider other) {
        // If instantiated prefabs collide with the saftey zone, invoke the stop event
        if (other.CompareTag("Player")) {          
            if (++stopped == 1) {
                SpeechManager.InvokeEvent("stop");
                zoneMat.SetColor("_Line_Color_", Color.red);
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        // If ALL instantiated prefabs no longer collide with the saftey zone, invoke the restart event
        if (stopped > 0 && other.CompareTag("Player")) {
            if (--stopped == 0) {
                SpeechManager.InvokeEvent("restart", true);
                zoneMat.SetColor("_Line_Color_", Color.green);
            }
        }
    }
}
