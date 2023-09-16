using UnityEngine;

public class LookAtUser : MonoBehaviour {
    // Rotate GameObject (usually UI) to face the user's head
    private void Update() {
        transform.LookAt(Camera.main.transform);
        transform.forward *= -1;
    }
}
