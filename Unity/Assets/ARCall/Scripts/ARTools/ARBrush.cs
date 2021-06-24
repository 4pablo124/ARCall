using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARBrush : MonoBehaviour
{

    [SerializeField] private PeerType myPeerType = PeerType.Host;

    [SerializeField] private GameObject prefab;
    
    private Camera arCam;
    private InputManager inputManager;
    private ARRaycastManager arRaycastManager;
    private LineRenderer line;

    private void Start()
    {
        arCam = GameObject.Find("ARCamera").GetComponent<Camera>();
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
        arRaycastManager = GameObject.Find("ARSessionOrigin").GetComponent<ARRaycastManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(inputManager.IsHeldDown(myPeerType)){
            List<ARRaycastHit> hitResults = new List<ARRaycastHit>(); 
            Vector3 screenPoint = myPeerType == PeerType.Host ?
                                    inputManager.hostPosition :
                                    inputManager.clientPosition;

            if(arRaycastManager.Raycast(arCam.ScreenPointToRay(screenPoint),hitResults,TrackableType.PlaneWithinPolygon)){
                Pose hitPose = hitResults[0].pose;
                if(line == null) {
                    line = createLineStart(hitPose.position, Color.red);
                }else{
                    drawNextPointInLine(line,hitPose.position);
                }
            }
        }else if(line != null) {
            GameObject.Instantiate(line).transform.parent = myPeerType == PeerType.Host ?  
                                                                ARToolController.hostDrawings.transform :
                                                                ARToolController.clientDrawings.transform;
            Destroy(line.gameObject);
        }
    }

    private LineRenderer drawNextPointInLine(LineRenderer line, UnityEngine.Vector3 point){
        line.SetPosition(line.positionCount++,point);
        line.Simplify(0.00001f);
        return line;
    }

    private LineRenderer createLineStart(UnityEngine.Vector3 start,Color color){
        LineRenderer line = GameObject.Instantiate(prefab, start, UnityEngine.Quaternion.identity).GetComponent<LineRenderer>();
        line.endColor = line.startColor = color;
        line.SetPosition(0,start);
        return line;
    }
}
