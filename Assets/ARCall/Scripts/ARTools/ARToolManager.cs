using UnityEngine;

public class ARToolManager : MonoBehaviour
{    
    public bool recording = false;

    [HideInInspector] public Material hostMaterial, clientMaterial;
    public Material ColorRed, ColorBlue, ColorGreen, ColorYellow;
    public GameObject guidePrefab;
    public static GameObject hostDrawings;
    public static GameObject clientDrawings;
    public static GameObject hostGuides;
    public static GameObject clientGuides;


    private  GameObject hostTools, clientTools;


    // Start is called before the first frame update
    void Awake()
    {

        hostDrawings = GameObject.Find("HostDrawings");
        clientDrawings = GameObject.Find("ClientDrawings");

        hostGuides = GameObject.Find("HostGuides");
        clientGuides = GameObject.Find("ClientGuides");

        hostTools = GameObject.Find("HostTools");
        clientTools = GameObject.Find("ClientTools");
        
        SelectHostTool("ARBrush");
        if(!recording) SelectClientTool("ARBrush");
    }

    public void SelectHostTool(string toolName){

        foreach(Transform tool in hostTools.transform){
            tool.gameObject.SetActive(false);
        }

        hostTools.transform.Find(toolName).gameObject.SetActive(true);
    }

    public void SelectClientTool(string toolName){

        foreach(Transform tool in clientTools.transform){
            tool.gameObject.SetActive(false);
        }

        clientTools.transform.Find(toolName).gameObject.SetActive(true);
    }

    public void UndoDrawing(string peer){
        switch (peer){
            case "host":
                if(hostDrawings.transform.childCount > 0){
                    Destroy(hostDrawings.transform.GetChild(hostDrawings.transform.childCount-1).gameObject);
                    Destroy(hostGuides.transform.GetChild(hostDrawings.transform.childCount-1).gameObject);
                }
            break;
            case "client":
                if(clientDrawings.transform.childCount > 0){
                    Destroy(clientDrawings.transform.GetChild(clientDrawings.transform.childCount-1).gameObject);
                    Destroy(clientGuides.transform.GetChild(hostDrawings.transform.childCount-1).gameObject);
                }
            break;
        }
    }

    public void DeleteDrawings(string peer){
        if(peer == "host" || peer == "both"){
            foreach(Transform child in hostDrawings.transform){
                Destroy(child.gameObject);
            }
            foreach(Transform child in hostGuides.transform){
                Destroy(child.gameObject);
            }
        }
        if(peer == "client" || peer == "both"){
            foreach(Transform child in clientDrawings.transform){
                Destroy(child.gameObject);
            }
            foreach(Transform child in clientGuides.transform){
                Destroy(child.gameObject);
            }
        }
    }


    public void changeHostColor(string color){
        switch(color){
            case "red" : hostMaterial = ColorRed; break;
            case "green" : hostMaterial = ColorGreen; break;
            case "blue" : hostMaterial = ColorBlue; break;
            case "yellow" : hostMaterial = ColorYellow; break;
        }
    }
    public void changeClientColor(string color){
        switch(color){
            case "red" : clientMaterial = ColorRed; break;
            case "green" : clientMaterial = ColorGreen; break;
            case "blue" : clientMaterial = ColorBlue; break;
            case "yellow" : clientMaterial = ColorYellow; break;
        }
    }

    public void placeGuide(PeerType peer, Transform target){
        var guide = GameObject.Instantiate(guidePrefab);
        var parent = peer == PeerType.Host ? hostGuides : clientGuides;

        guide.transform.SetParent(parent.transform);
        guide.GetComponent<ARGuide>().target = target;
        guide.GetComponent<SpriteRenderer>().color = peer == PeerType.Host ?
            hostMaterial.color : clientMaterial.color;
    }
}
