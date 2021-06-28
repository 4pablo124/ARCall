using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{   
    [SerializeField] public PeerType peerType = PeerType.Host;
    [SerializeField] public TextMeshProUGUI roomIDText;
    [SerializeField] public TMP_InputField roomIDInput;
    [SerializeField] public TextMeshProUGUI errorText;
    [SerializeField] public Button joinButton;
    public static string roomID;

    // Start is called before the first frame update
    async void Start()
    {
        if(peerType == PeerType.Host){
            roomIDText.text = await PersistentData.GenerateRoomID();
        }

        joinButton.onClick.AddListener(JoinRoom);
    }

    private void Update() {
        if(peerType == PeerType.Host){
            joinButton.interactable = roomIDText.text == "----" ? false : true;
            roomID = roomIDText.text;
        }else{
            joinButton.interactable = roomIDInput.text == "" ? false : true;
            roomID = roomIDInput.text;
        }
    }
    public async void JoinRoom(){
        if(peerType == PeerType.Host){
            PersistentData.SetRoomID(roomID);
            Debug.Log($"Entrando en sala: {roomID}");
            UINavigation.loadScene("Host");
        }else{
            //TODO: filtrar entrada de texto
            var snapshot = await FirebaseDatabase.DefaultInstance.GetReference("Rooms").Child(roomID).GetValueAsync();
            if(snapshot.Exists){
                errorText.gameObject.SetActive(false);
                PersistentData.SetRoomID(roomID);
                UINavigation.loadScene("Client");
            }else{
                errorText.gameObject.SetActive(true);
            }
        }
    }

    public static void shareRoom(){
        new NativeShare().SetTitle("ARCall Room ID").SetText("Unete a mi sala:\n"+roomID).Share();
    }
}
