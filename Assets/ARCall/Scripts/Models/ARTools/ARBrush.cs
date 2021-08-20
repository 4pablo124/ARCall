using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARBrush : MonoBehaviour
{

    [SerializeField] private PeerType myPeerType = PeerType.Host;

    [SerializeField] private GameObject linePrefab;

    private Camera arCam;
    private MyInputManager inputManager;
    private ARRaycastManager arRaycastManager;
    private ARToolManager aRToolManager;
    private LineRenderer line;

    private void Start()
    {
        arCam = GameObject.Find("ARCamera").GetComponent<Camera>();
        inputManager = GameObject.Find("InputManager").GetComponent<MyInputManager>();
        arRaycastManager = GameObject.Find("ARSessionOrigin").GetComponent<ARRaycastManager>();
        aRToolManager = GameObject.Find("ARToolManager").GetComponent<ARToolManager>();
    }

    // Update is called once per frame
    void Update()
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



    private LineRenderer drawNextPointInLine(LineRenderer line, UnityEngine.Vector3 point)
    {
        line.SetPosition(line.positionCount++, point);
        line.Simplify(0.00001f);
        return line;
    }

    private LineRenderer createLineStart(UnityEngine.Vector3 start)
    {
        LineRenderer line = GameObject.Instantiate(linePrefab, start, UnityEngine.Quaternion.identity).GetComponent<LineRenderer>();
        line.GetComponent<Renderer>().material = myPeerType == PeerType.Host ?
            aRToolManager.hostMaterial : aRToolManager.clientMaterial;
        line.SetPosition(0, start);
        return line;
    }
}
