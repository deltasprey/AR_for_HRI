using UnityEngine;

public class LookAtUser : MonoBehaviour {
    private void Update() {
        transform.LookAt(Camera.main.transform);
        transform.forward *= -1;
    }
}
