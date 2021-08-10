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
    [HideInInspector] public bool videoDisabled = false;
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
        videoRawImage = GameObject.Find("VideoRawImage").GetComponent<RawImage>();
        noVideoCanvas = GameObject.Find("NoVideo")?.GetComponent<Canvas>();
        arToolTipsUI = GameObject.Find("ARToolTipsUI")?.GetComponentInChildren<Canvas>();
    }


    private void Start() {
        if(isRecording) RecordCamera();
    }

    public void RecordCamera(){       
        aspectRatio = arCam.aspect;
        height = (int)Math.Round(width/aspectRatio);
        mainCam = arCam; 
        Debug.Log(mainCam);
        if(!isRecording) videoStream = mainCam.CaptureStream(width, height, (int)bitrate);

        if(mainCam == arCam) videoRawImage.texture = arCam.targetTexture;

        noVideoCanvas.worldCamera = mainCam;

        videoRawImage.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
        videoRawImage.color = Color.white;
        videoRawImage.texture.filterMode = FilterMode.Trilinear;
    }


    private void OnApplicationPause(bool paused) {
        if(paused){
            ShowVideo(false);
        }else{
            ShowVideo(true);
        }
    }

    public void ShowVideo(bool show){
        noVideoCanvas.enabled = !show;
        ARToolManager.hostDrawings.SetActive(show);
        ARToolManager.hostGuides.SetActive(show);
        ARToolManager.clientDrawings.SetActive(show);
        ARToolManager.clientGuides.SetActive(show);
        // arToolTipsUI.gameObject.SetActive(show);
        if(arSession != null) arSession.enabled = show;
        videoDisabled = !show;
    }

    // TODO: Enviar mensaje al cliente 
    public bool ToggleVideo(){

        if(!videoDisabled){
            ShowVideo(false);
        }else{
            ShowVideo(true);
        }

        return videoDisabled;
    }

}