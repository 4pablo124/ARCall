using System;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{

    public event Action<string> OnClientInput;
    [SerializeField] private PeerType myPeerType = PeerType.Host;
    [SerializeField] private Canvas clientCanvas;
    [SerializeField] private RawImage clientVideoRawImage;


    [HideInInspector] public Vector3 hostInput, clientInput;
    private float scaledPixelRatioX,scaledPixelRatioY, clientAspectRatio;
    private int croppedScreenWidth, croppedScreenHeight, offsetX, offsetY;


    // Start is called before the first frame update
    void Start()
    {  
        hostInput.z = clientInput.z = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0)){
            clientAspectRatio = (float)Screen.width/Screen.height;

            croppedScreenWidth = clientAspectRatio < PeerConnection.aspectRatio ?
                (int)Math.Round(PeerConnection.height*clientAspectRatio) : PeerConnection.width;

            croppedScreenHeight = clientAspectRatio > PeerConnection.aspectRatio ?
                (int)Math.Round(PeerConnection.width/clientAspectRatio) : PeerConnection.height;

            scaledPixelRatioX = (float)Screen.width/croppedScreenWidth;
            scaledPixelRatioY = (float)Screen.height/croppedScreenHeight;

            offsetX = (int)Math.Round( ((float)(PeerConnection.width - croppedScreenWidth)/2) * scaledPixelRatioX );
            offsetY = (int)Math.Round( ((float)(PeerConnection.height - croppedScreenHeight)/2) * scaledPixelRatioY );

            // clientScaledPixelRatio = (float)Screen.width/PeerConnection.width;
            if(myPeerType == PeerType.Host){
                hostInput.x = Input.mousePosition.x/scaledPixelRatioX;
                hostInput.y = Input.mousePosition.y/scaledPixelRatioY;
                hostInput.z = 19.99f;

            }else{
                // var sizeDelta = clientVideoRawImage.sizeDelta;
                // var canvasScale = new Vector2(clientCanvas.transform.localScale.x, clientCanvas.transform.localScale.y);
                // var finalScale = new Vector2(sizeDelta.x * canvasScale.x, sizeDelta.y * canvasScale.y);

                clientInput.x = (Input.mousePosition.x + offsetX) /scaledPixelRatioX;
                clientInput.y = (Input.mousePosition.y + offsetY) /scaledPixelRatioY;
                clientInput.z = 19.99f;

                Debug.Log($"x: {PeerConnection.width} y: {PeerConnection.height} || " +
                          $"x: {croppedScreenWidth} y: {croppedScreenHeight} || " +
                          $"x: {clientInput.x} y: {clientInput.y}");

                OnClientInput?.Invoke(JsonUtility.ToJson(clientInput));
            }
            
        }

        if(Input.GetMouseButtonUp(0)){
            if(myPeerType == PeerType.Host){
                hostInput.z = 0;
            }else{
                clientInput.z = 0;
                OnClientInput?.Invoke(JsonUtility.ToJson(clientInput));
            }
        }
    }
}
