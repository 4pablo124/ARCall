using System;
using UnityEngine;

public class Pointer : MonoBehaviour
{    
 
    public InputManager inputManager;
    private int cursorWidth;
    private GameObject hostPointer, clientPointer;

    private void Start () {      
        hostPointer = transform.GetChild(0).gameObject;
        clientPointer = transform.GetChild(1).gameObject;
    }
    private void Update () {
        cursorWidth = (int)Math.Round(Screen.width*0.1f);
        hostPointer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cursorWidth);
        hostPointer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cursorWidth);
        clientPointer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cursorWidth);
        clientPointer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cursorWidth);

        hostPointer.transform.position = 
            GetComponent<Canvas>().worldCamera.ScreenToWorldPoint(inputManager.hostInput);
        clientPointer.transform.position =
            GetComponent<Canvas>().worldCamera.ScreenToWorldPoint(inputManager.clientInput);

        hostPointer.GetComponent<SpriteRenderer>().enabled = inputManager.hostInput.z != 0;
        clientPointer.GetComponent<SpriteRenderer>().enabled = inputManager.clientInput.z != 0;
    }
 
}
