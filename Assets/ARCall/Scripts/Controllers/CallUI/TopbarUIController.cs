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
    private Button roomBtn;
    private AudioManager audioManager;
    private VideoManager videoManager;
    private PeerConnection peerConnection;

    private bool userReady = false;

    private void Awake()
    {
        muteMyAudioBtn = GameObject.Find("MuteMyAudio").GetComponent<Button>();
        disableVideoBtn = GameObject.Find("DisableVideo")?.GetComponent<Button>();
        otherUserBubble = GameObject.Find("Bubble");

        roomBtn = GameObject.Find("RoomBtn").GetComponent<Button>();

        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        videoManager = GameObject.Find("VideoManager").GetComponent<VideoManager>();
        peerConnection = GameObject.Find("PeerConnection").GetComponent<PeerConnection>();
    }

    // Start is called before the first frame update
    void Start()
    {
        muteMyAudioBtn.onClick.AddListener(() =>
        {
            var icon = muteMyAudioBtn.transform.Find("Icon").GetComponent<Image>();
            if (audioManager.ToggleMute(audioManager.inputAudioSource))
            {
                icon.sprite = muteAudioIcon;
                icon.color = Colors.GetColor("red");
            }
            else
            {
                icon.sprite = audioIcon;
                icon.color = Colors.GetColor("black");
            }
        });

        disableVideoBtn?.onClick.AddListener(() =>
        {
            var icon = disableVideoBtn.transform.Find("Icon").GetComponent<Image>();
            if (videoManager.ToggleVideo())
            {
                icon.sprite = disableVideoIcon;
                icon.color = Colors.GetColor("red");
            }
            else
            {
                icon.sprite = videoIcon;
                icon.color = Colors.GetColor("black");
            }
        });

        roomBtn.onClick.AddListener(() =>
        {
            SharingManager.ShareRoom();
        });

        otherUserBubble.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (userReady)
            {
                var icon = otherUserBubble.transform.Find("Icon").GetComponent<Image>();
                if (audioManager.ToggleMute(audioManager.outputAudioSource))
                {
                    icon.sprite = muteAudioIcon;
                    icon.color = Colors.GetColor("red");
                }
                else
                {
                    icon.sprite = audioIcon;
                    icon.color = Colors.GetColor("black");
                }
            }
        });


        peerConnection.OnUserConnected += () =>
        {
            userReady = true;
            otherUserBubble.transform.Find("Icon").rotation = Quaternion.identity;
            otherUserBubble.transform.Find("Icon").GetComponent<Image>().sprite = audioIcon;
        };


    }

    void FixedUpdate()
    {
        if (!userReady) otherUserBubble.transform.Find("Icon").Rotate(Vector3.forward * 6);
    }
}
