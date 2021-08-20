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


    private GameObject hostTools, clientTools;


    // Start is called before the first frame update
    void Awake()
    {
        hostDrawings = GameObject.Find("HostDrawings");
        clientDrawings = GameObject.Find("ClientDrawings");

        hostGuides = GameObject.Find("HostGuides");
        clientGuides = GameObject.Find("ClientGuides");

        hostTools = GameObject.Find("HostTools");
        clientTools = GameObject.Find("ClientTools");

        SelectTool(PeerType.Host, "ARBrush");
        if (!recording) SelectTool(PeerType.Client, "ARBrush");
    }

    public void SelectTool(PeerType peer, string toolName)
    {
        switch (peer)
        {
            case PeerType.Host:
                foreach (Transform tool in hostTools.transform)
                {
                    tool.gameObject.SetActive(false);
                }
                hostTools.transform.Find(toolName).gameObject.SetActive(true);
                break;
            case PeerType.Client:
                foreach (Transform tool in clientTools.transform)
                {
                    tool.gameObject.SetActive(false);
                }
                clientTools.transform.Find(toolName).gameObject.SetActive(true);
                break;
        }

    }

    public void UndoDrawing(string peer)
    {
        switch (peer)
        {
            case "Host":
                if (hostDrawings.transform.childCount > 0)
                {
                    Destroy(hostDrawings.transform.GetChild(hostDrawings.transform.childCount - 1).gameObject);
                    Destroy(hostGuides.transform.GetChild(hostDrawings.transform.childCount - 1).gameObject);
                }
                break;
            case "Client":
                if (clientDrawings.transform.childCount > 0)
                {
                    Destroy(clientDrawings.transform.GetChild(clientDrawings.transform.childCount - 1).gameObject);
                    Destroy(clientGuides.transform.GetChild(hostDrawings.transform.childCount - 1).gameObject);
                }
                break;
        }
    }

    public void DeleteDrawings(string peer)
    {
        if (peer == "Host" || peer == "Both")
        {
            foreach (Transform child in hostDrawings.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in hostGuides.transform)
            {
                Destroy(child.gameObject);
            }
        }
        if (peer == "Client" || peer == "Both")
        {
            foreach (Transform child in clientDrawings.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in clientGuides.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }


    //TODO: Quitar colores "hardcoded"   
    public void SelectColor(PeerType peer, string color)
    {
        switch (peer)
        {
            case PeerType.Host:
                switch (color)
                {
                    case "DC6B6D": hostMaterial = ColorRed; break;
                    case "6BDC99": hostMaterial = ColorGreen; break;
                    case "6BD4DC": hostMaterial = ColorBlue; break;
                    case "FFF64A": hostMaterial = ColorYellow; break;
                }
                break;
            case PeerType.Client:
                switch (color)
                {
                    case "DC6B6D": clientMaterial = ColorRed; break;
                    case "6BDC99": clientMaterial = ColorGreen; break;
                    case "6BD4DC": clientMaterial = ColorBlue; break;
                    case "FFF64A": clientMaterial = ColorYellow; break;
                }
                break;
        }
    }

    public GameObject PlaceGuide(PeerType peer, Transform target)
    {
        var guide = GameObject.Instantiate(guidePrefab);
        var parent = peer == PeerType.Host ? hostGuides : clientGuides;

        guide.transform.SetParent(parent.transform);
        guide.GetComponent<ARGuide>().target = target;
        guide.GetComponent<SpriteRenderer>().color = peer == PeerType.Host ?
            hostMaterial.color : clientMaterial.color;

        return guide;
    }
}
