using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayspaceControl : MonoBehaviour {
    List<Transform> objects = new();
    List<Vector3> objInit = new();
    
    private void Start() {
        foreach (Transform obj in GetComponentsInChildren<Transform>()) {
            objects.Add(obj);
            objInit.Add(obj.transform.position);
        }
    }

    private void OnTriggerExit(Collider other) {
        for (int i = 0; i < objects.Count; i++) {
            if (other.transform.name == objects[i].name) {
                other.transform.position = objInit[i];
                if (other.GetComponent<Rigidbody>()) {
                    other.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                }
            }
        }
    }
}
