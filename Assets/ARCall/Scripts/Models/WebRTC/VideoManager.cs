using System;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class VideoManager : MonoBehaviour
{

    // public PeerType myPeerType = PeerType.Host;
    public int width = 360;
    public int height = 640;
    public float aspectRatio = 0.5f;
    public ulong bitrate = 800;
    public uint framerate = 30;

    public bool isRecording = false;


    [HideInInspector] public RawImage videoRawImage;
    [HideInInspector] public VideoStreamTrack videoStreamTrack;
    [HideInInspector] public bool videoDisabled = false;
    [HideInInspector] public Camera mainCam;
    [HideInInspector] public ARSession arSession;

    private Camera arCam, webCam;
    private RawImage webCamRawImage;
    private WebCamTexture webcamTexture;
    private GameObject noVideoAR, noVideoUI;
    private Canvas arToolTipsUI;



    private void Awake()
    {
        arSession = GameObject.Find("ARSession")?.GetComponent<ARSession>();
        arCam = GameObject.Find("ARCamera")?.GetComponent<Camera>();
        videoRawImage = GameObject.Find("VideoRawImage").GetComponent<RawImage>();
        noVideoAR = transform.Find("NoVideoAR")?.gameObject;
        noVideoUI = transform.Find("NoVideoUI")?.gameObject;
        arToolTipsUI = GameObject.Find("ARToolTipsUI")?.GetComponentInChildren<Canvas>();
    }


    private void Start()
    {
        StartCoroutine(ARSession.CheckAvailability());
        if (isRecording) RecordCamera();
    }

    public VideoStreamTrack RecordCamera()
    {
        mainCam = arCam;
        aspectRatio = mainCam.aspect;
        height = (int)Math.Round(width / aspectRatio);

        if (!isRecording) videoStreamTrack = mainCam.CaptureStreamTrack(width, height, (int)bitrate * 1000, RenderTextureDepth.DEPTH_16);

        if (mainCam == arCam) videoRawImage.texture = arCam.targetTexture;

        noVideoAR.GetComponent<Canvas>().worldCamera = mainCam;

        videoRawImage.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
        videoRawImage.color = Color.white;
        videoRawImage.texture.filterMode = FilterMode.Trilinear;
        return videoStreamTrack;
    }


    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            ShowVideo(false);
        }
        else
        {
            ShowVideo(true);
        }
    }

    public void ShowVideo(bool show)
    {
        // videoStreamTrack.Enabled = show;
        noVideoAR.SetActive(!show);
        noVideoUI.SetActive(!show);
        ARToolManager.hostDrawings.SetActive(show);
        ARToolManager.hostGuides.SetActive(show);
        ARToolManager.clientDrawings.SetActive(show);
        ARToolManager.clientGuides.SetActive(show);
        // arToolTipsUI.gameObject.SetActive(show);
        if (arSession != null) arSession.enabled = show;
        videoDisabled = !show;
    }

    // TODO: Enviar mensaje al cliente 
    public bool ToggleVideo()
    {

        if (!videoDisabled)
        {
            ShowVideo(false);
        }
        else
        {
            ShowVideo(true);
        }

        return videoDisabled;
    }

}