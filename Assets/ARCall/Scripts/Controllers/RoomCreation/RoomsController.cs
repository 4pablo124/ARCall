using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomsController : MonoBehaviour
{   
    public PeerType peerType = PeerType.Host;
    private TextMeshProUGUI roomIDText;
    private TMP_InputField roomIDInput;
    private Button joinBtn;
    private Button shareBtn;


    private void Awake() {
        roomIDText = GameObject.Find("RoomIDText")?.GetComponent<TextMeshProUGUI>();
        roomIDInput = GameObject.Find("RoomIDInput")?.GetComponent<TMP_InputField>();
        joinBtn = GameObject.Find("JoinBtn").GetComponent<Button>();
        shareBtn = GameObject.Find("ShareBtn")?.GetComponent<Button>();
    }

    // Start is called before the first frame update
    async void Start() {
        if(peerType == PeerType.Host){
            roomIDText.text = await RoomManager.GenerateRoomID();
            shareBtn.onClick.AddListener(() => Sharing.ShareRoom());
            joinBtn.onClick.AddListener(() => JoinRoom(roomIDText.text));
        }else{
            joinBtn.onClick.AddListener(() => JoinRoom(roomIDInput.text));
            roomIDInput.onSubmit.AddListener((roomID) => JoinRoom(roomID));
        }


    }

    void Update() {
        if(peerType == PeerType.Host) {
            shareBtn.interactable = IsValidRoomCode();
        }
        joinBtn.interactable = IsValidRoomCode();
    }

    async void JoinRoom(string roomID){
        RoomManager.RoomID = roomID;
        if(!await RoomManager.JoinRoom(peerType)){
            AndroidUtils.ShowToast("¡Código de sala incorrecto!");
        }
    }

    bool IsValidRoomCode(){
        if(peerType == PeerType.Client){
            return roomIDInput.text.Length == 4;
        }else{
            return roomIDText.text != "----";
        }
    }
}
