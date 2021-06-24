using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{

    public event Action<string> OnClientInput;
    [SerializeField] private PeerType myPeerType = PeerType.Host;

    public Vector3 hostPosition, clientPosition;

    private float scaledPixelRatioX,scaledPixelRatioY, clientAspectRatio;
    private int croppedScreenWidth, croppedScreenHeight, offsetX, offsetY;


    // Start is called before the first frame update
    void Start()
    {  
        hostPosition.z = clientPosition.z = 0;
    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetMouseButton(0) && EventSystem.current.currentSelectedGameObject == null){
            clientAspectRatio = (float)Screen.width/Screen.height;

            croppedScreenWidth = clientAspectRatio < PeerConnection.aspectRatio ?
                (int)Math.Round(PeerConnection.height*clientAspectRatio) : PeerConnection.width;

            croppedScreenHeight = clientAspectRatio > PeerConnection.aspectRatio ?
                (int)Math.Round(PeerConnection.width/clientAspectRatio) : PeerConnection.height;

            scaledPixelRatioX = (float)Screen.width/croppedScreenWidth;
            scaledPixelRatioY = (float)Screen.height/croppedScreenHeight;

            offsetX = (int)Math.Round( ((float)(PeerConnection.width - croppedScreenWidth)/2) * scaledPixelRatioX );
            offsetY = (int)Math.Round( ((float)(PeerConnection.height - croppedScreenHeight)/2) * scaledPixelRatioY );


            if(myPeerType == PeerType.Host){
                hostPosition.x = Input.mousePosition.x/scaledPixelRatioX;
                hostPosition.y = Input.mousePosition.y/scaledPixelRatioY;
                hostPosition.z = 19.99f;

            }else{
                clientPosition.x = (Input.mousePosition.x + offsetX) /scaledPixelRatioX;
                clientPosition.y = (Input.mousePosition.y + offsetY) /scaledPixelRatioY;
                clientPosition.z = 19.99f;

                OnClientInput?.Invoke(JsonUtility.ToJson(clientPosition)); 
            }
            
        }

        if(Input.GetMouseButtonUp(0)){
            if(myPeerType == PeerType.Host){
                hostPosition.z = 0;
            }else{
                clientPosition.z = 0;
                OnClientInput?.Invoke(JsonUtility.ToJson(clientPosition));
            }
        }
    }

    public bool IsHeldDown(PeerType peer){
        switch (peer){
            case PeerType.Host: return hostPosition.z != 0;
            case PeerType.Client: return clientPosition.z != 0;

            default: return false;
        }
    }

}
