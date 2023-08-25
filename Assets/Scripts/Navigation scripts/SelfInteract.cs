using UnityEngine;

public class SelfInteract : MonoBehaviour {
    public delegate void FocusEvent(SelfInteract marker);
    public static event FocusEvent removeMe;
    public static event FocusEvent ignoreMe;

    public void onHover() {
        removeMe.Invoke(this);
    }

    public void onHoverExit() {
        ignoreMe.Invoke(this);
    }
}