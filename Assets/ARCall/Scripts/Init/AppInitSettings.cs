using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppInitSettings : MonoBehaviour
{
    void Awake(){
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        // ApplicationChrome.navigationBarState = ApplicationChrome.States.Visible;
        // ApplicationChrome.statusBarState = ApplicationChrome.States.Visible;
        // ApplicationChrome.statusBarTextColor = ApplicationChrome.StatusBarTextColor.Default;
    }
}
