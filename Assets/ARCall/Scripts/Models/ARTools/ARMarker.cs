using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Modela el comportamiento de la herramienta ARMarker
/// </summary>
public class ARMarker : MonoBehaviour
{
    [SerializeField] private PeerType myPeerType = PeerType.Host;
    [SerializeField] private GameObject prefab;


    private Camera cam;
    private MyInputManager inputManager;
    private ARRaycastManager arRaycastManager;
    private ARToolManager aRToolManager;

    private bool placingMarker = false;

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
    /// <para>Comprueba si se selecciona un elemento reconocido del entorno y coloca el marcador</para>
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
                    var marker = AddMarker(hitPose.position);
                    aRToolManager.PlaceGuide(myPeerType, marker.transform);
                }
            }
        }
        else
        {
            placingMarker = false;
        }

    }

    /// <summary>
    /// Coloca el marcador en la posicion seleccionada
    /// </summary>
    /// <param name="position">Posici??n tridimensional donde colocar el marcador</param>
    /// <returns>Marcador</returns>
    private GameObject AddMarker(Vector3 position)
    {
        var marker = GameObject.Instantiate(prefab, position, Quaternion.identity);
        int count = 0;
        switch (myPeerType)
        {
            case PeerType.Host:
                placingMarker = true;
                marker.tag = "HostMarker";
                marker.transform.GetChild(1).GetComponent<Renderer>().material = aRToolManager.hostMaterial;
                marker.transform.parent = ARToolManager.hostDrawings.transform;
                foreach (Transform child in ARToolManager.hostDrawings.transform)
                {
                    if (child.gameObject.tag == "HostMarker") count++;
                }
                break;

            case PeerType.Client:
                placingMarker = true;
                marker.tag = "ClientMarker";
                marker.transform.GetChild(1).GetComponent<Renderer>().material = aRToolManager.clientMaterial;
                marker.transform.parent = ARToolManager.clientDrawings.transform;
                foreach (Transform child in ARToolManager.clientDrawings.transform)
                {
                    if (child.gameObject.tag == "ClientMarker") count++;
                }
                break;
        }
        marker.GetComponentInChildren<TextMeshPro>().text = count.ToString();

        return marker;
    }
}
