
using System.Collections;
using UnityEngine;

public class WindowResView : MonoBehaviour
{    
    int lastWidth;
    int lastHeight;

    bool isReseting = false; 

    float aspect;
    float currentAspect;

    private void Awake() {
        DontDestroyOnLoad(this.gameObject);
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        aspect = (float) Screen.width/Screen.height;
    }

    void LateUpdate () {
        currentAspect = (float) Screen.width/Screen.height;

        if (currentAspect != aspect && !isReseting) {
            StartCoroutine(ChangeResolution());
        }
    }

    IEnumerator ChangeResolution(){
        isReseting = true;
        if (Screen.width != lastWidth) {
            // user is resizing width
            var heightAccordingToWidth = Screen.width / aspect;
            Screen.SetResolution(Screen.width, (int)Mathf.Round((float)heightAccordingToWidth), false);
            lastWidth = Screen.width;
        } else if(Screen.height != lastHeight){
            // user is resizing height
            var widthAccordingToHeigth = Screen.height * aspect;
            Screen.SetResolution((int)Mathf.Round((float)widthAccordingToHeigth), Screen.height, false);
            lastHeight = Screen.height;
        }

        yield return new WaitForSeconds(0.5f);
        isReseting = false;
    }
}