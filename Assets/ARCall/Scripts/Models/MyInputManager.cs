using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MyInputManager : MonoBehaviour
{
    public event Action<string> OnClientInput;
    public PeerType myPeerType = PeerType.Host;
    public bool recording = false;

    [HideInInspector] public Vector3 hostPosition, clientPosition;

    private float scaledPixelRatioX, scaledPixelRatioY, clientAspectRatio;
    private int croppedScreenWidth, croppedScreenHeight, offsetX, offsetY;

    private VideoManager videoManager;

    [HideInInspector] public TouchScreenKeyboard clientKeyboard;


    private void Awake()
    {
        videoManager = GameObject.Find("VideoManager")?.GetComponent<VideoManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        hostPosition.z = clientPosition.z = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && EventSystem.current.currentSelectedGameObject == null)
        {
            if (recording)
            {
                hostPosition.x = Input.mousePosition.x;
                hostPosition.y = Input.mousePosition.y;
                hostPosition.z = 19.99f;
            }
            else
            {
                clientAspectRatio = (float)Screen.width / Screen.height;

                croppedScreenWidth = clientAspectRatio < videoManager.aspectRatio ?
                    (int)Math.Round(videoManager.height * clientAspectRatio) : videoManager.width;

                croppedScreenHeight = clientAspectRatio > videoManager.aspectRatio ?
                    (int)Math.Round(videoManager.width / clientAspectRatio) : videoManager.height;

                scaledPixelRatioX = (float)Screen.width / croppedScreenWidth;
                scaledPixelRatioY = (float)Screen.height / croppedScreenHeight;

                offsetX = (int)Math.Round(((float)(videoManager.width - croppedScreenWidth) / 2) * scaledPixelRatioX);
                offsetY = (int)Math.Round(((float)(videoManager.height - croppedScreenHeight) / 2) * scaledPixelRatioY);


                if (myPeerType == PeerType.Host)
                {
                    hostPosition.x = Input.mousePosition.x / scaledPixelRatioX;
                    hostPosition.y = Input.mousePosition.y / scaledPixelRatioY;
                    hostPosition.z = 19.99f;

                }
                else
                {
                    clientPosition.x = (Input.mousePosition.x + offsetX) / scaledPixelRatioX;
                    clientPosition.y = (Input.mousePosition.y + offsetY) / scaledPixelRatioY;
                    clientPosition.z = 19.99f;

                    OnClientInput?.Invoke(JsonUtility.ToJson(clientPosition));
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (myPeerType == PeerType.Host)
            {
                hostPosition.z = 0;
            }
            else
            {
                clientPosition.z = 0;
                OnClientInput?.Invoke(JsonUtility.ToJson(clientPosition));
            }
        }

    }

    public bool IsHeldDown(PeerType peer)
    {
        switch (peer)
        {
            case PeerType.Host: return hostPosition.z != 0;
            case PeerType.Client: return clientPosition.z != 0;

            default: return false;
        }
    }

}
