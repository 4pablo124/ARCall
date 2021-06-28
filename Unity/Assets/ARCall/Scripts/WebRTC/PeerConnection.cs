using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Firebase.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using Unity.WebRTC;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class PeerConnection : MonoBehaviour
{
    public PeerType peerType = PeerType.Host; // Para poder inicializar el valor statico desde el editor
    public static PeerType myPeerType;

    [SerializeField] private Button roomIDBtn;
    private InputManager inputManager;
    private VideoManager videoManager;
    private AudioManager audioManager;
    private ARSession aRSession;

    private DatabaseReference database;
    private RTCDataChannel audioDataChannel, remoteAudioDataChannel, clientInputDataChannel, aspectRatioDataChannel;
    private RTCConfiguration RTCconfig;
    public static RTCPeerConnection pc;
    public static List<RTCRtpSender> pcSenders;


    private void Awake()
    {
        myPeerType = peerType;

        if(ImHost()) aRSession = GameObject.Find("ARSession").GetComponent<ARSession>();
            
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
        videoManager = GameObject.Find("VideoManager").GetComponent<VideoManager>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        //Establecemos ID de la sala
        RoomManager.roomID = PersistentData.GetRoomID();
        roomIDBtn.GetComponentInChildren<TextMeshProUGUI>().text = RoomManager.roomID;

        // Obetemos referencia a la base de datos
        database = FirebaseDatabase.DefaultInstance.GetReference("Rooms").Child(RoomManager.roomID);
        Debug.Log($"{myPeerType} - Obtenida referencia de database: {database}");


        // Inicializamos los callbacks
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // Host realiza llamada cuando llegue el cliente
        if(ImHost()) database.Child("Client").ChildAdded += (sender, args) => { StartCoroutine(Call()); };

        // Cuando recibimos un mensaje invocamos de manera asincrona la lectura del mismo
        database.Child("Messages").ChildAdded += (sender, args) => { StartCoroutine(ReadMessageDB(args)); };


        // Inicializamos WebRTC con decodificacion por software, ya que hardware aun no se soporta en android por ahora
        WebRTC.Initialize(EncoderType.Software);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Señalizamos que el peer esta listo
        if(ImHost()){
            database.Child("Host").Child("Ready").SetValueAsync(true);
        }else{
            database.Child("Client").Child("Ready").SetValueAsync(true);     
        }
        // Evitamos que la pantalla se apague
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Configuramos servidores ICE
        RTCconfig = ServersConfig();

        // Creamos los senders
        pcSenders = new List<RTCRtpSender>();    
 
        // Creamos nuestro peer
        pc = new RTCPeerConnection(ref RTCconfig);

        // Cuando se encuentre un Ice Candidate
        pc.OnIceCandidate = candidate => {
            SendMessageDB(myPeerType,new Data{ ice = InitIceCandidate(candidate) });
            Debug.Log($"{myPeerType} - Ice Enviado: {candidate.Candidate}");
        };

        // Cuando cambie el estado de la conexion
        pc.OnIceConnectionChange = state => { 
            Debug.Log($"{myPeerType} - IceConnectionState: {state}");
        };


        if(ImHost()) videoManager.RecordCamera();

        //pc.GetSenders().First().SetParameters(SetBandwidth(pc.GetSenders().First().GetParameters(), 1000000, 125000));

        // Añadimos los diferentes tracks
        addAudioTracks();
        addDataChannels();
        addVideoTracks();

    }


    // MAIN CONECCTION

    private void addAudioTracks(){

        // Creamos audioData
        RTCDataChannelInit conf = new RTCDataChannelInit();
        audioDataChannel = pc.CreateDataChannel("audio", conf);
        // Se envia el audio una vez codificado a bytes
        audioManager.OnEncoded += (data, length) => {
            if(audioManager.audioConected) audioDataChannel.Send(audioManager.Encode(data,length));
        };
        //PROVISIONAL
        audioDataChannel.OnOpen = () => { audioManager.audioConected = true; };
        audioDataChannel.OnClose = () => { audioManager.audioConected = false; };
    }

    private void addDataChannels(){

        // Creamos canales de Datos
        RTCDataChannelInit conf = new RTCDataChannelInit(){ordered = true, };

        if(!ImHost()){
            clientInputDataChannel = pc.CreateDataChannel("clientInput", conf);
            inputManager.OnClientInput += inputJson => {
                if(clientInputDataChannel.ReadyState == RTCDataChannelState.Open){
                    clientInputDataChannel.Send(inputJson);
                }
            };
        }

        aspectRatioDataChannel = pc.CreateDataChannel("aspectRatio", conf);
        aspectRatioDataChannel.OnOpen = () => {
            if(ImHost()) {
                aspectRatioDataChannel.Send(BitConverter.GetBytes(videoManager.aspectRatio));
            }
        };

        pc.OnDataChannel = channel => {
            switch(channel.Label){
                case "audio": // Recibimos data (y audio gracias a UnityOpus)  
                    remoteAudioDataChannel = channel;
                    remoteAudioDataChannel.OnMessage = bytes => { audioManager.Decode(bytes); };
                    break;

                case "clientInput":
                    clientInputDataChannel = channel;
                        clientInputDataChannel.OnMessage = bytes => {
                            var inputJson = System.Text.Encoding.UTF8.GetString(bytes);
                            inputManager.clientPosition = JsonUtility.FromJson<Vector3>(inputJson);
                        };
                    break;

                case "aspectRatio":
                    aspectRatioDataChannel = channel;
                    if (!ImHost()){
                        aspectRatioDataChannel.OnMessage = bytes => {
                            videoManager.aspectRatio = BitConverter.ToSingle(bytes,0);
                            videoManager.height = (int)Math.Round(videoManager.width/videoManager.aspectRatio);
                            Debug.Log($"Recibiendo aspect ratio de: {videoManager.aspectRatio}");
                            videoManager.videoRawImage.GetComponent<AspectRatioFitter>().aspectRatio = videoManager.aspectRatio;
                        };
                    }
                    break;
            }
        };
    }
    private void addVideoTracks(){

        if(ImHost()) {
            foreach (var track in videoManager.videoStream.GetTracks())
                {
                    pcSenders.Add(pc.AddTrack(track, videoManager.videoStream));
                }
                
            var parameters = pcSenders.First().GetParameters();
            foreach (var encoding in parameters.encodings)
            {
                // encoding.maxBitrate = videoManager.bitrate;
                // encoding.maxFramerate = videoManager.framerate;
            }
        }else{
            // Recibimos video
            pc.OnTrack = e => videoManager.videoStream.AddTrack(e.Track);
            videoManager.videoStream = new MediaStream();
            videoManager.videoStream.OnAddTrack = e => {
                if (e.Track is VideoStreamTrack track){
                    videoManager.videoRawImage.texture = track.InitializeReceiver(videoManager.width, videoManager.height);
                    videoManager.videoRawImage.color = Color.white;
                }
            };
        }

        // Inciamos actualizacion de "frames"
        if (!videoManager.videoUpdateStarted)
        {
            StartCoroutine(WebRTC.Update());
            videoManager.videoUpdateStarted = true;
        }
    }

    private IEnumerator Call(){

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





    // CALLBACKS
    private void OnSceneUnloaded<Scene>(Scene scene){
        UnReadyUser();
    }

    private void OnApplicationPause(bool paused) {
        if (paused){ UnReadyUser(); }
        else{ ReadyUser(); }
    }

    private void OnApplicationFocus(bool focused) {
        if (focused){ ReadyUser(); }
        else{ UnReadyUser(); }
    }

    private void OnDestroy(){
        aRSession?.Reset();
        // Desarmamos webRTC en destructor
        Debug.Log("WebRTC.Dispose()");
        WebRTC.Dispose();
    }



    // DATABASE AUX 
    private void SendMessageDB(PeerType peerType, Data data){
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
    private void UnReadyUser(){
        // Quitamos al peer de la base de datos, se eliminara la sala automaticamente cuando no haya peers
        database.Child(myPeerType.ToString()).RemoveValueAsync();
        // Devolvemos la pantalla a su estado original
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    private void ReadyUser(){
        database.Child(myPeerType.ToString()).Child("Ready").SetValueAsync(true);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }





    // CONNECTION AUX

    // Devuelve la configuracion de Servidores ICE y protocolos de transmision
    private static RTCConfiguration ServersConfig() {
        RTCConfiguration config = default;
        config.iceServers = new[] {
            new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}},
            new RTCIceServer {urls = new[] {"stun:global.stun.twilio.com:3478?transport=udp"}},
            new RTCIceServer {urls = new[] {"stun:stun.services.mozilla.com"}},
            new RTCIceServer {urls = new[] {"turn:numb.viagenie.ca"}, username = "pablo.delosriosges@alum.uca.es", credential = "QCG%Q$x6XnzwNq"}
            };

        return config;
    }

    // Genera la informacion necesaria para inicializar un IceCandidate
    private RTCIceCandidateInit InitIceCandidate(RTCIceCandidate candidate){
        RTCIceCandidateInit iceCandidate = new RTCIceCandidateInit();
        iceCandidate.candidate = candidate.Candidate;
        iceCandidate.sdpMid = candidate.SdpMid;
        iceCandidate.sdpMLineIndex = candidate.SdpMLineIndex;
        return iceCandidate;
    }


    // MISC AUX

    public static bool ImHost(){
        return myPeerType == PeerType.Host;
    }
}
