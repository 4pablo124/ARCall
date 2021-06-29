using System;
using System.Linq;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class VideoManager : MonoBehaviour {

    public event Action OnCamReady; 
    public int width = 360;
    public int height = 640;
    public float aspectRatio = 0.5f;
    public ulong bitrate = 1000000;
    public uint framerate = 30;

    
    [HideInInspector] public RawImage videoRawImage;
    [HideInInspector] public MediaStream videoStream;
    [HideInInspector] public bool videoUpdateStarted = false;
    [HideInInspector] public bool showingVideo = true;



    private ARSession arSession;
    private Camera arCam, webCam;
    public static Camera mainCam;
    private RawImage webCamRawImage;
    private WebCamTexture webcamTexture;



    private void Awake() {
        arSession = GameObject.Find("ARSession")?.GetComponent<ARSession>();
        arCam = GameObject.Find("ARCamera")?.GetComponent<Camera>();
        webCam = GameObject.Find("WebCamera")?.GetComponent<Camera>();
        videoRawImage = GameObject.Find("VideoRawImage").GetComponent<RawImage>();


        if(PeerConnection.ImHost()){
            ARSession.stateChanged += changeCamera;
            StartCoroutine(ARSession.CheckAvailability());  
        } 
    }
    private void changeCamera(ARSessionStateChangedEventArgs args) {
        if(args.state == ARSessionState.Ready){
            aspectRatio = arCam.aspect;
            height = (int)Math.Round(width/aspectRatio);
            mainCam = arCam;
            OnCamReady?.Invoke();
        }
        if(args.state == ARSessionState.Unsupported){
            arSession.enabled=false;
            webcamTexture = new WebCamTexture();
            webcamTexture.Play();
            mainCam = webCam;
            OnCamReady?.Invoke();
        }
    }

    public void RecordCamera(){       
        // Capturamos Video
        videoStream = mainCam.CaptureStream(width, height, (int)bitrate);

        if(mainCam == arCam) videoRawImage.texture = arCam.targetTexture;
        if(mainCam == webCam) videoRawImage.texture = webcamTexture;
        videoRawImage.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
        videoRawImage.color = Color.white;
        videoRawImage.texture.filterMode = FilterMode.Trilinear;
    }

    private void SetUpWebCam(){
        videoRawImage.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
        videoRawImage.texture = webcamTexture;
        videoRawImage.color = Color.white;
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();
    }

    private void SetUpArCam(){
        videoRawImage.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
        videoRawImage.texture = webcamTexture;
        videoRawImage.color = Color.white;
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();
    }

    // TODO: Enviar mensaje al cliente 
    public void ToggleVideo(){

        if(showingVideo){
            mainCam.gameObject.SetActive(false);
            videoRawImage.color = Color.clear;
        }else{
            mainCam.gameObject.SetActive(true);
            videoRawImage.color = Color.white;
        }
        showingVideo = !showingVideo;
    }
}