using System;
using System.Linq;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;

public class VideoManager : MonoBehaviour {

    public int width = 360;
    public int height = 640;
    public float aspectRatio = 0.5f;
    public ulong bitrate = 1000000;
    public uint framerate = 30;
    
    [HideInInspector] public RawImage videoRawImage;
    [HideInInspector] public MediaStream videoStream;
    [HideInInspector] public bool videoUpdateStarted = false;
    [HideInInspector] public bool showingVideo = true;



    private Camera arCam, webCam, mainCam;
    private RawImage webcamTexture;



    private void Awake() {
        arCam = GameObject.Find("ARCamera")?.GetComponent<Camera>();
        webCam = GameObject.Find("WebCamera")?.GetComponent<Camera>();
        webcamTexture = GameObject.Find("WebcamTexture")?.GetComponent<RawImage>();
        videoRawImage = GameObject.Find("VideoRawImage").GetComponent<RawImage>();
    }

    private void Start() {
        //TODO: Elegir camara en funcion de disponibilidad de ARCore

        if(PeerConnection.ImHost()){
            mainCam = arCam;
            aspectRatio = mainCam.aspect;
            height = (int)Math.Round(width/aspectRatio);
        }

    }

    public void RecordCamera(){
        
        // Capturamos Video
        Debug.Log($"Capturando video con aspect ratio de: {aspectRatio}");
        var height = (int)Math.Round(width/aspectRatio);
        videoStream = mainCam.CaptureStream(width, height, (int)bitrate);

        videoRawImage.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
        videoRawImage.texture = mainCam.targetTexture;
        videoRawImage.color = Color.white;
    }

    // public private MediaStream RecordWebcam(ref Camera camera, ref RawImage videoImage, ref RawImage cameraTexture){

    //     MediaStream video;
    //     // Capturamos Video
    //     Debug.Log($"Capturando video con aspect ratio de: {camera.aspect}");
    //     var webcam = new WebCamTexture();
    //     webcam.Play();
    //     Debug.Log("hi");
    //     cameraTexture.texture = webcam;
    //     cameraTexture.material.mainTexture = webcam;
    //     cameraTexture.GetComponent<AspectRatioFitter>().aspectRatio = (float)webcam.width/webcam.height;
        
    //     var height = (int)Math.Round(width/camera.aspect);
    //     video = camera.CaptureStream(width, height, (int)bitrate);


    //     videoImage.GetComponent<AspectRatioFitter>().aspectRatio = camera.aspect;
    //     videoImage.texture = camera.targetTexture;
    //     videoImage.color = Color.white;

    //     return video;
    // }

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