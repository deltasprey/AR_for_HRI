// Navigation marker interction events
using UnityEngine;

public class SelfInteract : MonoBehaviour {
    public delegate void FocusEvent(SelfInteract marker);
    public static event FocusEvent removeMe;
    public static event FocusEvent ignoreMe;
    public static event FocusEvent navigateToMe;

    public void onHover() { removeMe.Invoke(this); }

    public void onHoverExit() { ignoreMe.Invoke(this); }

    public void NavigateToMe() { navigateToMe?.Invoke(this); }
}