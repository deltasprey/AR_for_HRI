// Navigation marker interction events
using TMPro;
using UnityEngine;

public class SelfInteract : MonoBehaviour {
    public delegate void FocusEvent(SelfInteract marker);
    public static event FocusEvent removeMe;
    public static event FocusEvent ignoreMe;
    public static event FocusEvent navigateToMe;
    public static event FocusEvent pathToMe;
    public TMP_Text label;

    public void onHover() { removeMe.Invoke(this); }

    public void onHoverExit() { ignoreMe.Invoke(this); }

    public void NavigateToMe() { navigateToMe?.Invoke(this); }

    public void PathToMe() { pathToMe?.Invoke(this); }
}