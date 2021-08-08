using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarUIController : MonoBehaviour
{
    public PeerType peer = PeerType.Host;

    public bool isRecording = false;

    private Button undo;
    public GameObject deleteBar;
    private Button deleteBtn;
    private Button deleteMine;
    private Button deleteAll;
    public GameObject arToolsBar;
    private GameObject selectedTool;
    private GameObject tool1;
    private GameObject tool2;
    private GameObject tool3;
    public GameObject colorsBar;
    private GameObject selectedColor;
    private GameObject color1;
    private GameObject color2;
    private GameObject color3;
    private Button actionBtn;
    private PeerConnection peerConnection;
    private ARToolManager arToolManager;
    private ClientManager clientManager;
    private RecorderManager recorderManager;

    private void Awake() {
        //toolbar
        undo = GameObject.Find("Undo").GetComponent<Button>();
        deleteBtn = GameObject.Find("DeleteBtn").GetComponent<Button>();
        deleteMine = deleteBar.transform.Find("Buttons").Find("DeleteMine").GetComponent<Button>();
        deleteAll = deleteBar.transform.Find("Buttons").Find("DeleteAll").GetComponent<Button>();

        selectedTool = GameObject.Find("SelectedToolBtn");
        tool1 = arToolsBar.transform.Find("Buttons").Find("Tool1").gameObject;
        tool2 = arToolsBar.transform.Find("Buttons").Find("Tool2").gameObject;
        tool3 = arToolsBar.transform.Find("Buttons").Find("Tool3").gameObject;

        selectedColor = GameObject.Find("SelectedColorBtn");
        color1 = colorsBar.transform.Find("Buttons").Find("Color1").gameObject;
        color2 = colorsBar.transform.Find("Buttons").Find("Color2").gameObject;
        color3 = colorsBar.transform.Find("Buttons").Find("Color3").gameObject;

        actionBtn = GameObject.Find("ActionBtn")?.GetComponent<Button>();

        peerConnection = GameObject.Find("PeerConnection")?.GetComponent<PeerConnection>();
        arToolManager = GameObject.Find("ARToolManager")?.GetComponent<ARToolManager>();
        clientManager = GameObject.Find("ClientManager")?.GetComponent<ClientManager>();
        recorderManager = GameObject.Find("AndroidUtils")?.GetComponent<RecorderManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        undo.onClick.AddListener(() => {
            if(peer == PeerType.Host){
                arToolManager.UndoDrawing(peer.ToString());
            }else{
                clientManager.UndoDrawing();
            }
            
        });

        deleteBtn.onClick.AddListener(() => { 
            if(isRecording){
                arToolManager.DeleteDrawings(peer.ToString());
            }else{
                ToggleBar(deleteBar);   
            }
        });
        deleteMine.onClick.AddListener(() => { 
            if(peer == PeerType.Host){
                arToolManager.DeleteDrawings(peer.ToString());
            }else{
                clientManager.DeleteDrawings(peer.ToString());
            }
            ToggleBar(deleteBar);
        });
        deleteAll.onClick.AddListener(() => { 
            if(peer == PeerType.Host){
                arToolManager.DeleteDrawings("Both");
            }else{
                clientManager.DeleteDrawings("Both");
            }
            ToggleBar(deleteBar);
        });


        selectedTool.GetComponent<Button>().onClick.AddListener(() => { ToggleBar(arToolsBar); });
        tool1.GetComponent<Button>().onClick.AddListener(() => {
            SelectBarElement(arToolsBar,tool1);
            ToggleBar(arToolsBar);
        });
        tool2.GetComponent<Button>().onClick.AddListener(() => {
            SelectBarElement(arToolsBar,tool2);
            ToggleBar(arToolsBar);
        });
        tool3.GetComponent<Button>().onClick.AddListener(() => {
            SelectBarElement(arToolsBar,tool3);
            ToggleBar(arToolsBar);
        });
        
        selectedColor.GetComponent<Button>().onClick.AddListener(() => { ToggleBar(colorsBar); });
        color1.GetComponent<Button>().onClick.AddListener(() => {
            SelectBarElement(colorsBar,color1);
            ToggleBar(colorsBar);
        });
        color2.GetComponent<Button>().onClick.AddListener(() => {
            SelectBarElement(colorsBar,color2);
            ToggleBar(colorsBar);
        });
        color3.GetComponent<Button>().onClick.AddListener(() => {
            SelectBarElement(colorsBar,color3);
            ToggleBar(colorsBar);
        });

        actionBtn.onClick.AddListener(() => {
            var icon = actionBtn.transform.Find("Icon").GetComponent<Image>();
            if(isRecording){
                if(recorderManager.ToggleRecord()){
                    icon.pixelsPerUnitMultiplier = 40;
                }else{
                    icon.pixelsPerUnitMultiplier = 1;
                };
            }else{
                peerConnection.HangUp(); 
            }
                
        });

    }

    void SelectBarElement(GameObject bar, GameObject clickedElement){
        if(bar == arToolsBar){
            if(peer == PeerType.Host){
            arToolManager.SelectTool(peer, clickedElement.GetComponent<Image>().sprite.name);
            }else{
                clientManager.SelectTool(clickedElement.GetComponent<Image>().sprite.name);
            }
            // change sprite
            var sprite = clickedElement.GetComponent<Image>().sprite;
            clickedElement.GetComponent<Image>().sprite = selectedTool.GetComponent<Image>().sprite;
            selectedTool.GetComponent<Image>().sprite = sprite;

        }else if(bar == colorsBar){
            if(peer == PeerType.Host){
                arToolManager.SelectColor(peer, ColorUtility.ToHtmlStringRGB(clickedElement.GetComponent<Image>().color));
            }else{
                clientManager.SelectColor(ColorUtility.ToHtmlStringRGB(clickedElement.GetComponent<Image>().color));
            }
            // change color
            var color = clickedElement.GetComponent<Image>().color;
            clickedElement.GetComponent<Image>().color = selectedColor.GetComponent<Image>().color;
            selectedColor.GetComponent<Image>().color = color;
        }
    }

    void ToggleBar(GameObject bar){
        deleteBar.SetActive(bar == deleteBar ? !deleteBar.activeSelf : false);
        arToolsBar.SetActive(bar == arToolsBar ? !arToolsBar.activeSelf : false);
        colorsBar.SetActive(bar == colorsBar ? !colorsBar.activeSelf : false);
    }

}
