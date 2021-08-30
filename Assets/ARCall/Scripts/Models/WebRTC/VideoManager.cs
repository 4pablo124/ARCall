using System;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Maneja la lógica de la grabación y visualización de video
/// </summary>
public class VideoManager : MonoBehaviour
{

    // public PeerType myPeerType = PeerType.Host;

    /// <summary>
    /// Anchura del video
    /// </summary>
    public int width = 360;
    /// <summary>
    /// Altura del video
    /// </summary>
    public int height = 640;
    /// <summary>
    /// Relación de aspecto del video
    /// </summary>
    public float aspectRatio = 0.5f;
    /// <summary>
    /// Bitrate
    /// </summary>
    public ulong bitrate = 800;
    /// <summary>
    /// Fotogramas por segundo
    /// </summary>
    public uint framerate = 30;
    /// <summary>
    /// Indica si la sesión actual es una grabación
    /// </summary>
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



    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Inicializa los modelos y elementos de la interfaz</para>
    /// </summary>
    private void Awake()
    {
        arSession = GameObject.Find("ARSession")?.GetComponent<ARSession>();
        arCam = GameObject.Find("ARCamera")?.GetComponent<Camera>();
        videoRawImage = GameObject.Find("VideoRawImage").GetComponent<RawImage>();
        noVideoAR = transform.Find("NoVideoAR")?.gameObject;
        noVideoUI = transform.Find("NoVideoUI")?.gameObject;
        arToolTipsUI = GameObject.Find("ARToolTipsUI")?.GetComponentInChildren<Canvas>();
    }


    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// </summary>
    private void Start()
    {
        StartCoroutine(ARSession.CheckAvailability());
        if (isRecording) RecordCamera();
    }

    /// <summary>
    /// Graba la cámara de RA
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Llamada cuando el estado de pausa de la aplicación cambia
    /// <para>Alterna la visivilidad del video</para>
    /// </summary>
    /// <param name="paused">Estado de pausa</param>
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

    /// <summary>
    /// Establece la visibilidad del video
    /// </summary>
    /// <param name="show">Visibilidad del video</param>
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

    /// <summary>
    /// Alterna la visibilidad del video
    /// </summary>
    /// <returns>Visibilidad actual del video</returns>
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