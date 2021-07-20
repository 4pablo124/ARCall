using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomLoader : MonoBehaviour
{   
    public PeerType peerType = PeerType.Host;
    public TextMeshProUGUI roomIDText;
    public TMP_InputField roomIDInput;
    public TextMeshProUGUI errorText;
    public Button joinBtn;


    private void Awake() {
        roomIDText = GameObject.Find("RoomIDText")?.GetComponent<TextMeshProUGUI>();
        roomIDInput = GameObject.Find("RoomIDInput")?.GetComponent<TMP_InputField>();
        errorText = GameObject.Find("ErrorText")?.GetComponent<TextMeshProUGUI>();
        joinBtn = GameObject.Find("JoinBtn").GetComponent<Button>();
    }

    // Start is called before the first frame update
    void Start() {
        if(peerType == PeerType.Host){
            RoomManager.GenerateRoomID().ContinueWithOnMainThread( roomIDTask => {
                roomIDText.text = roomIDTask.Result;
                joinBtn.interactable = true;
            });
        }

        joinBtn.onClick.AddListener(() => {
            RoomManager.JoinRoom(peerType).ContinueWithOnMainThread(success =>{
                errorText.enabled = !success.Result;
            });
        });
    }

    private void Update() {
        if(peerType == PeerType.Client){
            joinBtn.interactable = roomIDInput.text.Length == 4;
        }
    }
}
