using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopbarUIController : MonoBehaviour
{

    public Sprite audioIcon;
    public Sprite muteAudioIcon;
    public Sprite videoIcon;
    public Sprite disableVideoIcon;

    private Button muteMyAudioBtn;
    private Button disableVideoBtn;
    private GameObject otherUserBubble;
    private GameObject myVolume;
    private GameObject otherVolume;
    private Button roomBtn;
    private MyRecorder audioInput;
    private MyPlayer audioOutput;
    private VideoManager videoManager;
    private PeerConnection peerConnection;

    private bool userReady = false;

    private void Awake() {
        muteMyAudioBtn = GameObject.Find("MuteMyAudio").GetComponent<Button>();
        disableVideoBtn = GameObject.Find("DisableVideo")?.GetComponent<Button>();
        otherUserBubble = GameObject.Find("Bubble");
        myVolume = GameObject.Find("MyVolume");
        otherVolume = GameObject.Find("otherVolume");

        roomBtn = GameObject.Find("RoomBtn").GetComponent<Button>();

        audioInput = GameObject.Find("AudioInput").GetComponent<MyRecorder>();
        audioOutput = GameObject.Find("AudioOutput").GetComponent<MyPlayer>();
        videoManager = GameObject.Find("VideoManager").GetComponent<VideoManager>();
        peerConnection = GameObject.Find("PeerConnection").GetComponent<PeerConnection>();
    }

    // Start is called before the first frame update
    void Start()
    {
        muteMyAudioBtn.onClick.AddListener(() => {
            var icon = muteMyAudioBtn.transform.Find("Icon").GetComponent<Image>();
            if(audioInput.ToggleMute()){
                icon.sprite = muteAudioIcon;
                icon.color = Colors.GetColor("red");
            }else{
                icon.sprite = audioIcon;
                icon.color = Colors.GetColor("black");
            }
        });

        disableVideoBtn?.onClick.AddListener(() => {
            var icon = disableVideoBtn.transform.Find("Icon").GetComponent<Image>();
            if(videoManager.ToggleVideo()){
                icon.sprite = disableVideoIcon;
                icon.color = Colors.GetColor("red");
            }else{
                icon.sprite = videoIcon;
                icon.color = Colors.GetColor("black");
            }
        });

        roomBtn.onClick.AddListener(() => {
            SharingManager.ShareRoom();
        });

        otherUserBubble.GetComponent<Button>().onClick.AddListener(() => {
            if(userReady){
                var icon = otherUserBubble.transform.Find("Icon").GetComponent<Image>();
                if(audioOutput.ToggleMute()){
                    icon.sprite = muteAudioIcon;
                    icon.color = Colors.GetColor("red");
                }else{
                    icon.sprite = audioIcon;
                    icon.color = Colors.GetColor("black");
                }
            }
        });


        peerConnection.OnUserConnected += () => {
            userReady = true;
            otherUserBubble.transform.Find("Icon").rotation = Quaternion.identity;
            otherUserBubble.transform.Find("Icon").GetComponent<Image>().sprite = audioIcon;
        };
        

    }

    void FixedUpdate()
    {
        if(!userReady) otherUserBubble.transform.Find("Icon").Rotate(Vector3.forward*6);
    }
}
