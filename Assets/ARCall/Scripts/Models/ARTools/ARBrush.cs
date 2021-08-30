using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Modela el comportamiento de la herramienta ARBrush
/// </summary>
public class ARBrush : MonoBehaviour
{
    [SerializeField] private PeerType myPeerType = PeerType.Host;

    [SerializeField] private GameObject linePrefab;

    private Camera arCam;
    private MyInputManager inputManager;
    private ARRaycastManager arRaycastManager;
    private ARToolManager aRToolManager;
    private LineRenderer line;

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Inicializa los modelos referenciados</para>
    /// </summary>
    private void Start()
    {
        arCam = GameObject.Find("ARCamera").GetComponent<Camera>();
        inputManager = GameObject.Find("InputManager").GetComponent<MyInputManager>();
        arRaycastManager = GameObject.Find("ARSessionOrigin").GetComponent<ARRaycastManager>();
        aRToolManager = GameObject.Find("ARToolManager").GetComponent<ARToolManager>();
    }

    /// <summary>
    /// Llamada al comienzo de cada fotograma
    /// <para>Comprueba si se selecciona un elemento reconocido del entorno y dibuja la linea</para>
    /// </summary>
    private void Update()
    {
        if (inputManager.IsHeldDown(myPeerType))
        {
            List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
            Vector3 screenPoint = myPeerType == PeerType.Host ?
                                    inputManager.hostPosition :
                                    inputManager.clientPosition;

            if (arRaycastManager.Raycast(arCam.ScreenPointToRay(screenPoint), hitResults, TrackableType.All))
            {
                Pose hitPose = hitResults[0].pose;
                if (line == null)
                {
                    line = createLineStart(hitPose.position);
                }
                else
                {
                    drawNextPointInLine(line, hitPose.position);
                }
            }
        }
        else if (line != null)
        {
            var lineClone = GameObject.Instantiate(line);
            lineClone.transform.parent = myPeerType == PeerType.Host ?
                                                                ARToolManager.hostDrawings.transform :
                                                                ARToolManager.clientDrawings.transform;

            aRToolManager.PlaceGuide(myPeerType, lineClone.transform);
            Destroy(line.gameObject);
        }
    }


    /// <summary>
    /// Coloca el siguiente punto en la linea que se esta dibujando actualmente
    /// </summary>
    /// <param name="line">Linea actual</param>
    /// <param name="point">Punto tridimensional que añadir</param>
    /// <returns></returns>
    private LineRenderer drawNextPointInLine(LineRenderer line, UnityEngine.Vector3 point)
    {
        line.SetPosition(line.positionCount++, point);
        line.Simplify(0.00001f);
        return line;
    }

    /// <summary>
    /// Crea el comienzo de la linea a dibujar
    /// </summary>
    /// <param name="start">Punto tridimensional donde comenzar la linea</param>
    /// <returns></returns>
    private LineRenderer createLineStart(UnityEngine.Vector3 start)
    {
        LineRenderer line = GameObject.Instantiate(linePrefab, start, UnityEngine.Quaternion.identity).GetComponent<LineRenderer>();
        line.GetComponent<Renderer>().material = myPeerType == PeerType.Host ?
            aRToolManager.hostMaterial : aRToolManager.clientMaterial;
        line.SetPosition(0, start);
        return line;
    }
}
