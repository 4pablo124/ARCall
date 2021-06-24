using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARMarker : MonoBehaviour
{
    [SerializeField] private PeerType myPeerType = PeerType.Host;
    [SerializeField] private  GameObject prefab;
    [SerializeField] private  Material hostMaterial, clientMaterial;


    private Camera cam;
    private InputManager inputManager;
    private ARRaycastManager arRaycastManager;
    private LineRenderer hostMarker;
    private bool placingMarker = false;
    
    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.Find("ARCamera").GetComponent<Camera>();
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
        arRaycastManager = GameObject.Find("ARSessionOrigin").GetComponent<ARRaycastManager>();
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
                    AddMarker(hitPose.position);             
                }
            }
        }else{
            placingMarker = false;
        }

    }

    private void AddMarker(Vector3 position){
        var marker = GameObject.Instantiate(prefab,position,Quaternion.identity);
        int count = 0;
        switch (myPeerType){
            case PeerType.Host:
                placingMarker = true;
                marker.tag = "HostMarker";
                marker.transform.GetChild(1).GetComponent<Renderer>().material = hostMaterial;
                marker.transform.parent = ARToolController.hostDrawings.transform;
                foreach (Transform child in ARToolController.hostDrawings.transform) {
                    if(child.gameObject.tag == "HostMarker") count++;
                } break;

            case PeerType.Client:
                placingMarker = true;
                marker.tag = "ClientMarker";
                marker.transform.GetChild(1).GetComponent<Renderer>().material = clientMaterial;
                marker.transform.parent = ARToolController.clientDrawings.transform;
                foreach (Transform child in ARToolController.clientDrawings.transform) {
                    if(child.gameObject.tag == "ClientMarker") count++;
                } break;
        }
        marker.GetComponentInChildren<TextMeshPro>().text = count.ToString();
    }
}
