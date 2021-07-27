using System;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class VideoManager : MonoBehaviour {

    // public PeerType myPeerType = PeerType.Host;
    public int width = 360;
    public int height = 640;
    public float aspectRatio = 0.5f;
    public ulong bitrate = 100000;
    public uint framerate = 30;

    public bool isRecording = false;

    
    [HideInInspector] public RawImage videoRawImage;
    [HideInInspector] public MediaStream videoStream;
    [HideInInspector] public bool videoUpdateStarted = false;
    [HideInInspector] public bool showingVideo = true;
    [HideInInspector] public Camera mainCam;
    [HideInInspector] public ARSession arSession;

    private Camera arCam, webCam;
    private RawImage webCamRawImage;
    private WebCamTexture webcamTexture;
    private Canvas noVideoCanvas;
    private Canvas arToolTipsUI;



    private void Awake() {
        arSession = GameObject.Find("ARSession")?.GetComponent<ARSession>();
        arCam = GameObject.Find("ARCamera")?.GetComponent<Camera>();
        webCam = GameObject.Find("WebCamera")?.GetComponent<Camera>();
        videoRawImage = GameObject.Find("VideoRawImage").GetComponent<RawImage>();
        noVideoCanvas = GameObject.Find("NoVideo")?.GetComponent<Canvas>();
        arToolTipsUI = GameObject.Find("ARToolTipsUI")?.GetComponentInChildren<Canvas>();
    }

    // private void Start() {
    //     width = Screen.width/2;
    //     height = Screen.height/2;
    // }

    // TODO: implementar webcam para no ar

    // private void changeCamera(ARSessionStateChangedEventArgs args) {
    //     if(args.state == ARSessionState.Ready || args.state == ARSessionState.SessionTracking){
    //         aspectRatio = arCam.aspect;
    //         height = (int)Math.Round(width/aspectRatio);
    //         mainCam = arCam;
    //     }
    //     if(args.state == ARSessionState.Unsupported){
    //         arSession.enabled=false;
    //         arToolTipsUI.gameObject.SetActive(false);
    //         webcamTexture = new WebCamTexture();
    //         webcamTexture.Play();
    //         mainCam = webCam;
    //     }
    //     ARSession.stateChanged -= changeCamera;
    //     RecordCamera();
    // }


    private void Start() {
        if(isRecording) RecordCamera();
    }

    public void RecordCamera(){       
        Debug.Log(arCam.targetTexture);
        aspectRatio = arCam.aspect;
        height = (int)Math.Round(width/aspectRatio);
        mainCam = arCam; 

        if(!isRecording) videoStream = mainCam.CaptureStream(width, height, (int)bitrate);

        if(mainCam == arCam) videoRawImage.texture = arCam.targetTexture;

        noVideoCanvas.worldCamera = mainCam;

        videoRawImage.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
        videoRawImage.color = Color.white;
        videoRawImage.texture.filterMode = FilterMode.Trilinear;
    }


    private void OnApplicationPause(bool paused) {
        Debug.Log("aplications pasuie");
        if(paused){
            ShowVideo(false);
        }else{
            ShowVideo(true);
        }
    }

    public void ShowVideo(bool show){
        noVideoCanvas.enabled = !show;
        ARToolManager.hostDrawings.gameObject.SetActive(show);
        ARToolManager.hostGuides.gameObject.SetActive(show);
        ARToolManager.clientDrawings.gameObject.SetActive(show);
        ARToolManager.clientGuides.gameObject.SetActive(show);
        arToolTipsUI.gameObject.SetActive(show);
        arSession.enabled = show;
        showingVideo = show;
    }

    // TODO: Enviar mensaje al cliente 
    public void ToggleVideo(){

        if(showingVideo){
            ShowVideo(false);
        }else{
            ShowVideo(true);
        }
    }

}