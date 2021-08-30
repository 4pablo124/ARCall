using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la interfaz de la creación y unión a videollamadas
/// </summary>
public class RoomsController : MonoBehaviour
{
    public PeerType peerType = PeerType.Host;
    private TextMeshProUGUI roomIDText;
    private TMP_InputField roomIDInput;
    private Button joinBtn;
    private Button shareBtn;


    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Inicializa los modelos y elementos de la interfaz</para>
    /// </summary>
    private void Awake()
    {
        roomIDText = GameObject.Find("RoomIDText")?.GetComponent<TextMeshProUGUI>();
        roomIDInput = GameObject.Find("RoomIDInput")?.GetComponent<TMP_InputField>();
        joinBtn = GameObject.Find("JoinBtn").GetComponent<Button>();
        shareBtn = GameObject.Find("ShareBtn")?.GetComponent<Button>();
    }

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Asigna las acciones a los botones de la interfaz</para>
    /// </summary>
    private async void Start()
    {
        if (peerType == PeerType.Host)
        {
            roomIDText.text = await RoomManager.GenerateRoomID();
            shareBtn.onClick.AddListener(() => SharingManager.ShareRoom());
            joinBtn.onClick.AddListener(() => JoinRoom(roomIDText.text));
        }
        else
        {
            joinBtn.onClick.AddListener(() => JoinRoom(roomIDInput.text));
            roomIDInput.onSubmit.AddListener((roomID) => JoinRoom(roomID));
        }


    }

    /// <summary>
    /// Llamada al comienzo de cada fotograma
    /// <para>Evalua la entrada de texto</para>
    /// </summary>
    private void Update()
    {
        if (peerType == PeerType.Host)
        {
            shareBtn.interactable = IsValidRoomCode();
        }
        joinBtn.interactable = IsValidRoomCode();
    }

    /// <summary>
    /// Lleva al usuario actual a la sala seleccionada
    /// </summary>
    /// <param name="roomID">Código de la sala seleccionada</param>
    async void JoinRoom(string roomID)
    {
        RoomManager.RoomID = roomID;
        if (!await RoomManager.JoinRoom(peerType))
        {
            AndroidUtils.ShowToast("¡Código de sala incorrecto!");
        }
    }

    /// <summary>
    /// Evalua si el código de la sala introducido es correcto
    /// </summary>
    /// <returns>La validez del código de sala introducido</returns>
    bool IsValidRoomCode()
    {
        if (peerType == PeerType.Client)
        {
            return roomIDInput.text.Length == 4;
        }
        else
        {
            return roomIDText.text != "----";
        }
    }
}
