using UnityEngine;

public class LookAtUser : MonoBehaviour {
    void Update() {
        transform.LookAt(Camera.main.transform);
        transform.forward *= -1;
    }
}
