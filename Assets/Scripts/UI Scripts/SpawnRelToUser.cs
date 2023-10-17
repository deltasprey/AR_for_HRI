using UnityEngine;

public class SpawnRelToUser : MonoBehaviour {
    [SerializeField] private Vector3 relOffset;

    private void OnEnable() {
        transform.parent.position = Camera.main.transform.position +
                                    Camera.main.transform.right.normalized * relOffset.x +
                                    Camera.main.transform.forward.normalized * relOffset.z +
                                    Camera.main.transform.up.normalized * relOffset.y;
        transform.parent.eulerAngles += new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
    }
}
