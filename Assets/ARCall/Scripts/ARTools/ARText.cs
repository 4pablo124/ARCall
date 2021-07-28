using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARText : MonoBehaviour
{

    public event Action OnARTextClick;

    [SerializeField] private PeerType myPeerType = PeerType.Host;
    [SerializeField] private  GameObject prefab;

    private Camera cam;
    private InputManager inputManager;
    private ARRaycastManager arRaycastManager;
    private LineRenderer hostMarker;
    private bool placingMarker = false;

    private TouchScreenKeyboard keyboard;
    private GameObject currentMarker;
    
    private ARToolManager aRToolManager;

    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.Find("ARCamera").GetComponent<Camera>();
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
        arRaycastManager = GameObject.Find("ARSessionOrigin").GetComponent<ARRaycastManager>();
        aRToolManager = GameObject.Find("ARToolManager").GetComponent<ARToolManager>();

    }

    // Update is called once per frame
    void Update()
    {
        if(inputManager.IsHeldDown(myPeerType)){
            if(!placingMarker){
                List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
                Vector3 screenPoint = myPeerType == PeerType.Host ?
                                    inputManager.hostPosition :
                                    inputManager.clientPosition;
                if(arRaycastManager.Raycast(cam.ScreenPointToRay(screenPoint),hitResults,TrackableType.PlaneWithinPolygon)){
                    Pose hitPose = hitResults[0].pose;
                    currentMarker = AddMarker(hitPose.position);   
                    aRToolManager.placeGuide(myPeerType, currentMarker.transform);
                }
            }
        }else if(currentMarker != null){
            switch (myPeerType){
                case PeerType.Host:
                    currentMarker.GetComponentInChildren<TextMeshPro>().text = keyboard.text;
                break;
                case PeerType.Client:
                    currentMarker.GetComponentInChildren<TextMeshPro>().text = inputManager.clientText;
                break;
            }
            placingMarker = false;
        }

    }

    private GameObject AddMarker(Vector3 position){
        var marker = GameObject.Instantiate(prefab,position,Quaternion.identity);
        placingMarker = true;
        switch (myPeerType){
            case PeerType.Host:
                marker.transform.GetChild(1).GetComponent<Renderer>().material = aRToolManager.hostMaterial;
                marker.transform.parent = ARToolManager.hostDrawings.transform;
                keyboard = TouchScreenKeyboard.Open("",TouchScreenKeyboardType.Default,true,true,false,false,
                                            "Introduzca su texto",36);
                
                break;

            case PeerType.Client:
                marker.transform.GetChild(1).GetComponent<Renderer>().material = aRToolManager.clientMaterial;
                marker.transform.parent = ARToolManager.clientDrawings.transform;
                OnARTextClick?.Invoke();
                // keyboard.text = inputManager.clientText;
                break;
        }
        return marker;
    }

    
}
