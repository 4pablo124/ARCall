using System;
using UnityEngine;

public class ClientPointer : MonoBehaviour
{    
 
    private Vector3 cursorPosition;
    private int cursorWidth;
    private float scaledPixelRatio;

    private void Start () {      
        cursorPosition.z = 19.99f;
        scaledPixelRatio = (float)Screen.width/PeerConnection.width;
        // scaledPixelRatio = 1;

    }
    private void Update () {
        cursorPosition.x = PeerConnection.clientInput.x/scaledPixelRatio;
        cursorPosition.y = PeerConnection.clientInput.y/scaledPixelRatio;
        

        cursorWidth = (int)Math.Round(Screen.width*0.1f);
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cursorWidth);
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cursorWidth);
        transform.position = GetComponentInParent<Canvas>().worldCamera.ScreenToWorldPoint(cursorPosition);

        GetComponent<SpriteRenderer>().enabled = PeerConnection.clientInput.z == 0;
    }
 
}
