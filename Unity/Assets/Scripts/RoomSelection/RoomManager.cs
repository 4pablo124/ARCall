using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{   
    [SerializeField] private PeerType peerType = PeerType.Host;
    [SerializeField] private TextMeshProUGUI roomIDText;
    [SerializeField] private TMP_InputField roomIDInput;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Button joinButton;

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
        }else{
            joinButton.interactable = roomIDInput.text == "" ? false : true;
        }
    }
    public async void JoinRoom(){
        if(peerType == PeerType.Host){
            PersistentData.SetRoomID(roomIDText.text);
            Debug.Log($"Entrando en sala: {roomIDText.text}");
            SceneManager.LoadScene("Host");
        }else{
            //TODO: filtrar entrada de texto
            var snapshot = await FirebaseDatabase.DefaultInstance.GetReference("Rooms").Child(roomIDInput.text).GetValueAsync();
            if(snapshot.Exists){
                errorText.gameObject.SetActive(false);
                PersistentData.SetRoomID(roomIDInput.text);
                SceneManager.LoadScene("Client");
            }else{
                errorText.gameObject.SetActive(true);
            }
        }
    }
}
