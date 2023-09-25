using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CamDisp : MonoBehaviour {
    public GameObject headsUpImg;
    public int refreshRate = 24; // FPS

    WebCamTexture webCam;
    Texture2D tex;

    void Start() {
        webCam = new() { requestedWidth = GetComponent<Camera>().targetTexture.width };
        webCam.Play();
        tex = new(webCam.width, webCam.height);

        StartCoroutine(LookTimer());
    }

    private void Update() { // Debugging only
        if (Input.GetKeyDown(KeyCode.I)) {
            headsUpImg.SetActive(!headsUpImg.activeSelf);
        }
    }

    private void LookForMarker() {
        tex.SetPixels(webCam.GetPixels(0, 0, webCam.width, webCam.height));
        tex.Apply();
        headsUpImg.GetComponent<RawImage>().texture = tex;
    }

    IEnumerator LookTimer() {
        while (true) {
            yield return new WaitForSeconds(1/(float)refreshRate);
            LookForMarker();
        }
    }
}
