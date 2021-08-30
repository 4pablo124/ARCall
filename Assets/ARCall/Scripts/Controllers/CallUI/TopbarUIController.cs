using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la interfaz de la barra de opciones superior en una videollamada
/// </summary>
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
    private PeerConnectionManager peerConnection;

    private bool userReady = false;

 
    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Inicializa los modelos y elementos de la interfaz</para>
    /// </summary>
    private void Awake()
    {
        muteMyAudioBtn = GameObject.Find("MuteMyAudio").GetComponent<Button>();
        disableVideoBtn = GameObject.Find("DisableVideo")?.GetComponent<Button>();
        otherUserBubble = GameObject.Find("Bubble");

        roomBtn = GameObject.Find("RoomBtn").GetComponent<Button>();

        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        videoManager = GameObject.Find("VideoManager").GetComponent<VideoManager>();
        peerConnection = GameObject.Find("PeerConnection").GetComponent<PeerConnectionManager>();
    }

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Asigna las acciones a los botones de la interfaz</para>
    /// </summary>
    private void Start()
    {
        muteMyAudioBtn.onClick.AddListener(() =>
        {
            var icon = muteMyAudioBtn.transform.Find("Icon").GetComponent<Image>();
            if (audioManager.ToggleMuteInput())
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
                if (audioManager.ToggleMuteOutput())
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

    /// <summary>
    /// Llamada cada fotograma, asegurandose de que no se salta ningun fotograma aunque se reduzca el rendimiento del sistema
    /// <para>Hace girar al icono de espera de conexión</para>
    /// </summary>
    void FixedUpdate()
    {
        if (!userReady) otherUserBubble.transform.Find("Icon").Rotate(Vector3.forward * 6);
    }
}
