using System;
using UnityEngine;

/// <summary>
/// Modela el comportamiento de la herramienta ARPointer
/// </summary>
public class ARPointer : MonoBehaviour
{
    [SerializeField] private PeerType myPeerType = PeerType.Host;

    private MyInputManager inputManager;
    private int cursorWidth;
    private GameObject pointer;

    private VideoManager videoManager;
    private ARToolManager aRToolManager;


    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Inicializa los modelos y elementos de la interfaz</para>
    /// </summary>
    private void Awake()
    {
        inputManager = GameObject.Find("InputManager")?.GetComponent<MyInputManager>();
        pointer = transform.GetChild(0).gameObject;
        videoManager = GameObject.Find("VideoManager")?.GetComponent<VideoManager>();
        // videoManager.OnCamReady += setUpCam;
        aRToolManager = GameObject.Find("ARToolManager").GetComponent<ARToolManager>();


    }

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Establece los valores iniciales</para>
    /// </summary>
    private void Start()
    {
        pointer.GetComponent<SpriteRenderer>().enabled = false;
    }

    /// <summary>
    /// Llamada al comienzo de cada fotograma
    /// Comprueba la posición del puntero y lo muestra
    /// </summary>
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

    // void setUpCam()
    // {
    //     GetComponent<Canvas>().worldCamera = videoManager.mainCam;
    // }
}
