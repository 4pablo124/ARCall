using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Modela el comportamiento de la herramienta ARText
/// </summary>
public class ARText : MonoBehaviour
{

    [SerializeField] private PeerType myPeerType = PeerType.Host;
    [SerializeField] private GameObject prefab;

    private Camera cam;
    private MyInputManager inputManager;
    private ARRaycastManager arRaycastManager;
    private LineRenderer hostMarker;
    private bool placingMarker = false;

    private TouchScreenKeyboard keyboard;
    private GameObject currentMarker;

    private ARToolManager aRToolManager;

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Inicializa los modelos referenciados</para>
    /// </summary>
    private void Start()
    {
        cam = GameObject.Find("ARCamera").GetComponent<Camera>();
        inputManager = GameObject.Find("InputManager").GetComponent<MyInputManager>();
        arRaycastManager = GameObject.Find("ARSessionOrigin").GetComponent<ARRaycastManager>();
        aRToolManager = GameObject.Find("ARToolManager").GetComponent<ARToolManager>();

    }

    /// <summary>
    /// Llamada al comienzo de cada fotograma
    /// <para>Comprueba si se selecciona un elemento reconocido del entorno y coloca el texto</para>
    /// </summary>
    private void Update()
    {
        if (inputManager.IsHeldDown(myPeerType))
        {
            if (!placingMarker)
            {
                List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
                Vector3 screenPoint = myPeerType == PeerType.Host ?
                                    inputManager.hostPosition :
                                    inputManager.clientPosition;
                if (arRaycastManager.Raycast(cam.ScreenPointToRay(screenPoint), hitResults, TrackableType.All))
                {
                    Pose hitPose = hitResults[0].pose;
                    currentMarker = AddMarker(hitPose.position);
                    aRToolManager.PlaceGuide(myPeerType, currentMarker.transform);
                }
            }
        }
        else if (currentMarker != null)
        {
            currentMarker.GetComponentInChildren<TextMeshPro>().text = keyboard.text;
            placingMarker = false;
        }

    }

    /// <summary>
    /// Coloca el texto en la posicion seleccionada
    /// </summary>
    /// <param name="position">Posición tridimensional donde colocar el texto</param>
    /// <returns>Texto</returns>
    private GameObject AddMarker(Vector3 position)
    {
        var marker = GameObject.Instantiate(prefab, position, Quaternion.identity);
        placingMarker = true;
        switch (myPeerType)
        {
            case PeerType.Host:
                marker.transform.GetChild(1).GetComponent<Renderer>().material = aRToolManager.hostMaterial;
                marker.transform.parent = ARToolManager.hostDrawings.transform;
                keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, true, true, false, false,
                                            "Introduzca su texto", 36);

                break;

            case PeerType.Client:
                marker.transform.GetChild(1).GetComponent<Renderer>().material = aRToolManager.clientMaterial;
                marker.transform.parent = ARToolManager.clientDrawings.transform;
                // OnARTextClick?.Invoke();
                // keyboard.text = inputManager.clientText;
                break;
        }
        return marker;
    }


}
