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
    void Start() {
        if(peerType == PeerType.Host){
            RoomManager.GenerateRoomID().ContinueWithOnMainThread( roomIDTask => {
                roomIDText.text = roomIDTask.Result;
            });
            shareBtn.onClick.AddListener(() => Sharing.ShareRoom());
            joinBtn.onClick.AddListener(() => JoinRoom(roomIDText.text));
        }else{
            joinBtn.onClick.AddListener(() => JoinRoom(roomIDInput.text));
            roomIDInput.onSubmit.AddListener((roomID) => JoinRoom(roomID));
        }


    }

    private void Update() {
        if(peerType == PeerType.Host) shareBtn.interactable = IsValidRoomCode();
        joinBtn.interactable = IsValidRoomCode();
    }

    void JoinRoom(string roomID){
        RoomManager.RoomID = roomID;
        RoomManager.JoinRoom(peerType).ContinueWithOnMainThread(success =>{
            if(!success.Result){
                AndroidUtils.ShowToast("¡Código de sala incorrecto!");
            }
        });
    }

    bool IsValidRoomCode(){
        if(peerType == PeerType.Client){
            return roomIDInput.text.Length == 4;
        }else{
            return roomIDText.text != "----";
        }
    }
}
