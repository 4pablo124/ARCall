using System;
using UnityEngine;

public class ARPointer : MonoBehaviour
{

    [SerializeField] private PeerType myPeerType = PeerType.Host;

    private MyInputManager inputManager;
    private int cursorWidth;
    private GameObject pointer;

    private VideoManager videoManager;
    private ARToolManager aRToolManager;


    private void Awake()
    {
        inputManager = GameObject.Find("InputManager")?.GetComponent<MyInputManager>();
        pointer = transform.GetChild(0).gameObject;
        videoManager = GameObject.Find("VideoManager")?.GetComponent<VideoManager>();
        // videoManager.OnCamReady += setUpCam;
        aRToolManager = GameObject.Find("ARToolManager").GetComponent<ARToolManager>();


    }

    private void Start()
    {
        pointer.GetComponent<SpriteRenderer>().enabled = false;
    }

    private void Update()
    {
        pointer.GetComponent<SpriteRenderer>().color = myPeerType == PeerType.Host ?
            aRToolManager.hostMaterial.color : aRToolManager.clientMaterial.color;
        cursorWidth = (int)Math.Round(Screen.width * 0.1f);
        pointer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cursorWidth);
        pointer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cursorWidth);

        Vector3 screenPoint = myPeerType == PeerType.Host ?
                                    inputManager.hostPosition :
                                    inputManager.clientPosition;

        pointer.transform.position = GetComponent<Canvas>().worldCamera.ScreenToWorldPoint(screenPoint);

        pointer.GetComponent<SpriteRenderer>().enabled = inputManager.IsHeldDown(myPeerType);
    }

    void setUpCam()
    {
        GetComponent<Canvas>().worldCamera = videoManager.mainCam;
    }
}
