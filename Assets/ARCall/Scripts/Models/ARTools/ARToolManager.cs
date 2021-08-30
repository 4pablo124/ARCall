using UnityEngine;

/// <summary>
/// Maneja la lógica de las herramientas de RA
/// </summary>
public class ARToolManager : MonoBehaviour
{
    /// <summary>
    /// Indica si la sesión actual es una grabación
    /// </summary>
    public bool recording = false;
    
    /// <summary>
    /// Material usado para las herramientas del host
    /// </summary>
    [HideInInspector] public Material hostMaterial;

    /// <summary>
    /// Material usado para las herramientas del cliente
    /// </summary>
    [HideInInspector] public Material clientMaterial;

    /// <summary>
    /// Material predeterminado rojo
    /// </summary>
    public Material ColorRed;
    /// <summary>
    /// Material predeterminado azul
    /// </summary>
    public Material ColorBlue;
    /// <summary>
    /// Material predeterminado verde
    /// </summary>
    public Material ColorGreen;
    /// <summary>
    /// Material predeterminado amarillo
    /// </summary>
    public Material ColorYellow;
    /// <summary>
    /// Prefab usado para las guias
    /// </summary>
    public GameObject guidePrefab;
    /// <summary>
    /// <see cref="GameObject"/> donde se guardan los trazos o marcadores del host
    /// </summary>
    public static GameObject hostDrawings;
    /// <summary>
    /// <see cref="GameObject"/> donde se guardan los trazos o marcadores del cliente
    /// </summary>
    public static GameObject clientDrawings;
    /// <summary>
    /// <see cref="GameObject"/> donde se guardan las guías del host
    /// </summary>
    public static GameObject hostGuides;
    /// <summary>
    /// <see cref="GameObject"/> donde se guardan guías del cliente
    /// </summary>
    public static GameObject clientGuides;

    /// <summary>
    /// <see cref="GameObject"/> donde se guardan guías del cliente
    /// </summary>
    private GameObject hostTools;
    /// <summary>
    /// <see cref="GameObject"/> donde se guardan guías del cliente
    /// </summary>
    private GameObject clientTools;


    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Inicializa los modelos referenciados</para>
    /// </summary>
    private void Awake()
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

    /// <summary>
    /// Selecciona la heramienta elegida para el par seleccionado
    /// </summary>
    /// <param name="peer">Par seleccionado</param>
    /// <param name="toolName">Nombre de la herramienta seleccionada</param>
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

    /// <summary>
    /// Elimina el último trazo o marcador colocado por el par seleccionado
    /// </summary>
    /// <param name="peer">Par seleccionado</param>
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

    /// <summary>
    /// Elimina todos los trazos o marcadores del par seleccionado
    /// </summary>
    /// <param name="peer">Par seleccionado</param>
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
  
    /// <summary>
    /// Selecciona el color actual para el par seleccionado
    /// </summary>
    /// <param name="peer">Par seleccionado</param>
    /// <param name="color">Color seleccionado</param>
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

    /// <summary>
    /// Coloca una guía en un trazo o marcador del par selecionado
    /// </summary>
    /// <param name="peer">Par seleccionado</param>
    /// <param name="target">Posición del trazo o marcador selecionado</param>
    /// <returns></returns>
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
