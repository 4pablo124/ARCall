using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARToolController : MonoBehaviour
{
    [SerializeField] private PeerType myPeerType = PeerType.Host;
    
    public static GameObject hostDrawings;
    public static GameObject clientDrawings;

    private  GameObject aRTools;
    private  GameObject aRBrush;
    private  GameObject aRPointer;
    private  GameObject aRMarker;
    private  GameObject aRText;




    // Start is called before the first frame update
    void Awake()
    {
        hostDrawings = ARToolController.hostDrawings = GameObject.Find("HostDrawings");
        clientDrawings = ARToolController.clientDrawings = GameObject.Find("ClientDrawings");

        aRTools = this.gameObject;
        aRBrush = transform.Find("ARBrush").gameObject;
        aRPointer = transform.Find("ARPointer").gameObject;
        aRMarker = transform.Find("ARMarker").gameObject;
        aRText = transform.Find("ARText").gameObject;
        
        SelectTool("ARBrush");
    }

    public void SelectTool(string toolName){
        foreach(Transform tool in aRTools.transform){
            tool.gameObject.SetActive(false);
        }

        switch (toolName) {
            case "ARBrush": aRBrush.SetActive(true); break;
            case "ARPointer": aRPointer.SetActive(true); break;
            case "ARMarker": aRMarker.SetActive(true); break;
            case "ARText": aRText.SetActive(true); break;
        }
    }
    
    public void UndoDrawing(){
        switch (myPeerType){
            case PeerType.Host:
                if(hostDrawings.transform.childCount > 0)
                    Destroy(hostDrawings.transform.GetChild(hostDrawings.transform.childCount-1).gameObject);
                break;
            case PeerType.Client:
                if(clientDrawings.transform.childCount > 0)
                    Destroy(clientDrawings.transform.GetChild(clientDrawings.transform.childCount-1).gameObject);
                break;
        }
    }

    public void DeleteDrawings(string peer){
        if(peer == "host" || peer == "both"){
            foreach(Transform child in hostDrawings.transform){
                Destroy(child.gameObject);
            }
        }
        if(peer == "client" || peer == "both"){
            foreach(Transform child in hostDrawings.transform){
                Destroy(child.gameObject);
            }
        }
    }
}
