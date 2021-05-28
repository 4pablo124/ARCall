using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using Firebase.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;

public class NewPeerConnection : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI roomIDText;
    [SerializeField]private PeerType myPeerType = PeerType.Host;
    [SerializeField] private Camera cam;
    [SerializeField] private RawImage videoImage;

    private String roomID;
    private DatabaseReference database;
    private MediaStream videoStream;
    private RTCConfiguration RTCconfig;
    private RTCPeerConnection pc;
    private List<RTCRtpSender> pcSenders;

    private bool videoUpdateStarted = false;
    private const int width = 720;
    private const int height = 1280;

    private void Awake()
    {
        // Inicializamos WebRTC con decodificacion por software, ya que da problemas en android
        WebRTC.Initialize(EncoderType.Software);

        //Establecemos ID de la sala
        roomID = PersistentData.GetRoomID();
        roomIDText.text = roomID;

        // Obetemos referencia a la base de datos
        database = FirebaseDatabase.DefaultInstance.GetReference("Rooms").Child(roomID);
        Debug.Log($"{myPeerType} - Obtenida referencia de database: {database}");

        // Configuramos servidores ICE
        RTCconfig = ServersConfig();
        Debug.Log($"{myPeerType} - Servidores configurados: {RTCconfig}");
    }

    private void OnDestroy()
    {
        // Desarmamos webRTC en destructor
        WebRTC.Dispose();
        database.Reference.RemoveValueAsync();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Esperamos a que esten todos los peers para comenzar la llamada
        if(myPeerType == PeerType.Host){
            database.Child("Host").Child("Ready").SetValueAsync(true);
            database.Child("Client").ChildAdded += (sender, args) => { StartCoroutine(Call()); };
        }else{
            database.Child("Client").Child("Ready").SetValueAsync(true);     
        }

        // Cuando recibimos un mensjasre invocamos de manera asyncrona a la lectura del mismo
        database.Child("Messages").ChildAdded += (sender, args) => { StartCoroutine(ReadMessageDB(args)); };

        // Creamos nuestro peer
        pcSenders = new List<RTCRtpSender>();
        pc = new RTCPeerConnection(ref RTCconfig);
        pc.OnIceCandidate = candidate => {OnIceCandidate(candidate);};
        pc.OnIceConnectionChange = state => { Debug.Log($"{myPeerType} - IceConnectionState: {state}"); };

        if(myPeerType == PeerType.Client){
            pc.OnTrack = e => videoStream.AddTrack(e.Track);    
            videoStream = new MediaStream();
            videoStream.OnAddTrack = e => {
                if (e.Track is VideoStreamTrack track)
                {
                    videoImage.texture = track.InitializeReceiver(width, height);
                    videoImage.color = Color.white;
                }
            };

            if (!videoUpdateStarted)
            {
                Debug.Log("WebRTC.Update() Started");
                StartCoroutine(WebRTC.Update());
                videoUpdateStarted = true;
            }
        }

        if(myPeerType == PeerType.Host){
            RecordLive();
        }

    }

    

    private void RecordLive(){
        if (videoStream == null)
        {
            videoStream = cam.CaptureStream(width, height, 1000000);
            Debug.Log($"{myPeerType} - Capturando stream: {videoStream}");
        }
            
        videoImage.texture = cam.targetTexture;

        videoImage.color = Color.white;

    }

    private void AddTracks()
    {   
        if(myPeerType == PeerType.Host){
            foreach (var track in videoStream.GetTracks())
            {
                pcSenders.Add(pc.AddTrack(track, videoStream));
            }
        }

        if (!videoUpdateStarted)
        {
            Debug.Log("WebRTC.Update() Started");
            StartCoroutine(WebRTC.Update());
            videoUpdateStarted = true;
        }
    }

    private IEnumerator Call(){
        AddTracks();

        Debug.Log($"{myPeerType} - Realizando conexion");

        var op = pc.CreateOffer();
        yield return op;
        Debug.Log($"{myPeerType} - Generada Offer: {!op.IsError}");

        var offer = op.Desc;
        var op2 = pc.SetLocalDescription(ref offer);
        yield return op2;
        Debug.Log($"{myPeerType} - Guardada Offer (LocalDescription): {!op2.IsError}");

        Debug.Log($"{myPeerType} - Enviando Offer: {!op2.IsError}");
        SendMessageDB(myPeerType, new Data{ sdp = pc.LocalDescription });
    }

    // Invocado cuando se añada un IceCandidate
    private void OnIceCandidate(RTCIceCandidate candidate){
        SendMessageDB(myPeerType,new Data{ ice = InitIceCandidate(candidate) });
        Debug.Log($"{myPeerType} - Ice Enviado: {candidate.Candidate}");
    }

    // Genera la informacion necesaria para inicializar un IceCandidate
    private RTCIceCandidateInit InitIceCandidate(RTCIceCandidate candidate){
        RTCIceCandidateInit iceCandidate = new RTCIceCandidateInit();
        iceCandidate.candidate = candidate.Candidate;
        iceCandidate.sdpMid = candidate.SdpMid;
        iceCandidate.sdpMLineIndex = candidate.SdpMLineIndex;
        return iceCandidate;
    }

    // Envia un mensaje mediante la base de datos
    private void SendMessageDB(PeerType peerType, Data data)
    {
        JObject messageJSON = JObject.FromObject(
            new Message{ peerType = peerType, data = data }
        );
        var msg = database.Child("Messages").Push().SetRawJsonValueAsync(messageJSON.ToString());
    }
    
    // Lee un mensaje mediante la base de datos, se invoca cada vez que se envie un mensaje
    private IEnumerator ReadMessageDB(ChildChangedEventArgs args){
        var msg = JsonConvert.DeserializeObject<Message>(args.Snapshot.GetRawJsonValue());
        args.Snapshot.Reference.RemoveValueAsync();
        //El mensaje es para mi
        if(myPeerType != msg.peerType){

            // Me envia Ice
            if(msg.data.ice != null){
                pc.AddIceCandidate(new RTCIceCandidate(msg.data.ice));
                Debug.Log($"{myPeerType} - Ice Añadido: {msg.data.ice.candidate}");
            }

            // Me envia Sdp (offer)
            else if (msg.data.sdp.type == RTCSdpType.Offer){
                // Guardamos offer
                var description = msg.data.sdp;
                var op = pc.SetRemoteDescription(ref description);
                yield return op;
                Debug.Log($"{myPeerType} - Guardada Offer (RemoteDescription): {!op.IsError}");

                // Creamos Answer
                var op2 = pc.CreateAnswer();
                yield return op2;
                Debug.Log($"{myPeerType} - Creada Answer: {!op2.IsError}");


                // La guardamos
                var answer = op2.Desc;
                var op3 = pc.SetLocalDescription(ref answer);
                yield return op3;
                Debug.Log($"{myPeerType} - Guardada Answer (LocalDescription): {!op3.IsError}");

                // Y la enviamos
                SendMessageDB(myPeerType, new Data{ sdp = pc.LocalDescription });
            }

            // Me envia Sdp (Answer)
            else if (msg.data.sdp.type == RTCSdpType.Answer){
                var answer = msg.data.sdp;
                var op4 = pc.SetRemoteDescription(ref answer);
                yield return op4;
                Debug.Log($"{myPeerType} - Guardada Answer (RemoteDescription): {!op4.IsError}");
                if(op4.IsError){
                    Debug.Log(op4.Error.message);
                }

            }
        }

    }

    // Devuelve la configuracion de Servidores ICE y protocolos de transmision
    private static RTCConfiguration ServersConfig()
    {
        RTCConfiguration config = default;
        config.iceServers = new[] {
            new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}},
            new RTCIceServer {urls = new[] {"stun:global.stun.twilio.com:3478?transport=udp"}},
            new RTCIceServer {urls = new[] {"stun:stun.services.mozilla.com"}},
            new RTCIceServer {urls = new[] {"turn:numb.viagenie.ca"}, username = "pablo.delosriosges@alum.uca.es", credential = "QCG%Q$x6XnzwNq"}
            };

        return config;
    }
}
