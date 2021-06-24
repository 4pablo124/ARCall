using System;
using UnityEngine;

public class ARPointer : MonoBehaviour
{    

    [SerializeField] private PeerType myPeerType = PeerType.Host;

    private InputManager inputManager;
    private int cursorWidth;
    private GameObject pointer;

    private void Start () { 
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();

        pointer = transform.GetChild(0).gameObject;
    }
    private void Update () {
        cursorWidth = (int)Math.Round(Screen.width*0.1f);
        pointer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cursorWidth);
        pointer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cursorWidth);

        Vector3 screenPoint = myPeerType == PeerType.Host ?
                                    inputManager.hostPosition :
                                    inputManager.clientPosition;

        pointer.transform.position = GetComponent<Canvas>().worldCamera.ScreenToWorldPoint(screenPoint);

        pointer.GetComponent<SpriteRenderer>().enabled = inputManager.IsHeldDown(myPeerType);
    }
 
}
