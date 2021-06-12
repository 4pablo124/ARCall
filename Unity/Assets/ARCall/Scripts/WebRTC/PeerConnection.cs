using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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

public class PeerConnection : MonoBehaviour
{

    [SerializeField] private Button roomIDBtn;
    [SerializeField] private PeerType myPeerType = PeerType.Host;
    [SerializeField] private Camera cam;
    [SerializeField] private RawImage videoImage;
    [SerializeField] private MyEncoder encoder;
    [SerializeField] private MyDecoder decoder;
    [SerializeField] private InputManager inputManager;


    private String roomID;
    private DatabaseReference database;
    private MediaStream videoStream;
    private MediaStream audioStream;
    private RTCDataChannel audioDataChannel, remoteAudioDataChannel, clientInputDataChannel, aspectRatioDataChannel;
    private RTCConfiguration RTCconfig;
    private RTCPeerConnection pc;
    private List<RTCRtpSender> pcSenders;

    private bool videoUpdateStarted = false;
    private byte[] audioBuffer;
    private bool audioConected = false;
    private bool showingVideo = true;

    public static int width = 360;
    public static int height = 640;
    public static ulong bitrate = 1000000;
    public static float aspectRatio;

    public static Vector3 clientInput;


    private void Awake()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // Evitamos que la pantalla se apague
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
     
        // Inicializamos WebRTC con decodificacion por software, ya que hardware aun no se soporta en android por ahora
        WebRTC.Initialize(EncoderType.Software);

        //Establecemos ID de la sala
        roomID = PersistentData.GetRoomID();
        roomIDBtn.GetComponentInChildren<TextMeshProUGUI>().text = roomID;

        // Obetemos referencia a la base de datos
        database = FirebaseDatabase.DefaultInstance.GetReference("Rooms").Child(roomID);
        Debug.Log($"{myPeerType} - Obtenida referencia de database: {database}");

        // Configuramos servidores ICE
        RTCconfig = ServersConfig();
        Debug.Log($"{myPeerType} - Servidores configurados: {RTCconfig.iceServers}");
    }

    // Start is called before the first frame update
    void Start()
    {
        // Esperamos a que esten todos los peers para comenzar la llamada
        if(ImHost()){
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
        pc.OnIceCandidate = candidate => {
            SendMessageDB(myPeerType,new Data{ ice = InitIceCandidate(candidate) });
            Debug.Log($"{myPeerType} - Ice Enviado: {candidate.Candidate}");
        };
        pc.OnIceConnectionChange = state => { 
            Debug.Log($"{myPeerType} - IceConnectionState: {state}");
        };

        //pc.GetSenders().First().SetParameters(SetBandwidth(pc.GetSenders().First().GetParameters(), 1000000, 125000));

        if(ImHost()){ 
            videoStream = VideoManager.RecordLiveVideo(ref cam, ref videoImage);
        }

        // Añadimos los diferentes tracks
        addAudioTracks();
        addDataChannels();
        addVideoTracks();

    }

    private void OnDestroy()
    {
        // Desarmamos webRTC en destructor
        Debug.Log("WebRTC.Dispose()");
        WebRTC.Dispose();
    }




    // MAIN CONECCTION

    private void addAudioTracks(){
        // Creamos audioData
        RTCDataChannelInit conf = new RTCDataChannelInit();
        audioDataChannel = pc.CreateDataChannel("audio", conf);
        // Enviamos el audio una vez este codificado
        encoder.OnEncoded += EncodeAndSendBytes;
        //PROVISIONAL
        audioDataChannel.OnOpen = () => { audioConected = true; };
        audioDataChannel.OnClose = () => { audioConected = false; };
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
            if(ImHost()) aspectRatioDataChannel.Send(BitConverter.GetBytes(cam.aspect));
        };

        pc.OnDataChannel = channel => {
            switch(channel.Label){
                case "audio": // Recibimos data (y audio gracias a UnityOpus)  
                    remoteAudioDataChannel = channel;
                    remoteAudioDataChannel.OnMessage = bytes => { Decode(bytes); };
                    break;

                case "clientInput":
                    clientInputDataChannel = channel;
                        clientInputDataChannel.OnMessage = bytes => {
                            var inputJson = System.Text.Encoding.UTF8.GetString(bytes);
                            inputManager.clientInput = JsonUtility.FromJson<Vector3>(inputJson);
                        };
                    break;

                case "aspectRatio":
                    aspectRatioDataChannel = channel;
                    if (!ImHost()){
                        aspectRatioDataChannel.OnMessage = bytes => {
                            aspectRatio = BitConverter.ToSingle(bytes,0);
                            height = (int)Math.Round(width/aspectRatio);
                            Debug.Log($"Recibiendo aspect ratio de: {aspectRatio}");
                            videoImage.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
                        };
                    }
                    break;
            }
        };
    }
    private void addVideoTracks(){
        if(ImHost()) {
            foreach (var track in videoStream.GetTracks())
                {
                    pcSenders.Add(pc.AddTrack(track, videoStream));
                }
                
            var parameters = pcSenders.First().GetParameters();
            foreach (var encoding in parameters.encodings)
            {
                encoding.maxBitrate = bitrate;
                encoding.maxFramerate = 30;
            }
        }else{
            // Recibimos video
            pc.OnTrack = e => videoStream.AddTrack(e.Track);    
            videoStream = new MediaStream();
            videoStream.OnAddTrack = e => {
                if (e.Track is VideoStreamTrack track){
                    videoImage.texture = track.InitializeReceiver(width, height);
                    videoImage.color = Color.white;
                }
            };
        }

        // Inciamos actualizacion de "frames"
        if (!videoUpdateStarted)
        {
            Debug.Log("WebRTC.Update() Started");
            StartCoroutine(WebRTC.Update());
            videoUpdateStarted = true;
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





    // DATABASE AUX 
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

    // Genera la informacion necesaria para inicializar un IceCandidate
    private RTCIceCandidateInit InitIceCandidate(RTCIceCandidate candidate){
        RTCIceCandidateInit iceCandidate = new RTCIceCandidateInit();
        iceCandidate.candidate = candidate.Candidate;
        iceCandidate.sdpMid = candidate.SdpMid;
        iceCandidate.sdpMLineIndex = candidate.SdpMLineIndex;
        return iceCandidate;
    }






    // MISC AUX
    public void shareRoom(){
        RoomManager.shareRoom(roomID);
    }

    public bool ImHost(){
        return myPeerType == PeerType.Host;
    }

    //TODO - mover a videomanager
    public void ToggleVideo(){
        if(showingVideo){
            cam.gameObject.SetActive(false);
            videoImage.color = Color.clear;
        }else{
            cam.gameObject.SetActive(true);
            videoImage.color = Color.white;
        }
        showingVideo = !showingVideo;
    }




    // ENCODING AUX
    void Decode(byte[] bytes)
    {
        if(bytes.Length != 1){
            int size = sizeof(int);
            byte[] encodedLengthBytes = bytes.Take(size).ToArray();
            byte[] encodedAudioBytes = bytes.Skip(sizeof(int)).Take(bytes.Length - sizeof(int)).ToArray();
            int length = DecodeLength(encodedLengthBytes);
            
            decoder.Decode(encodedAudioBytes, length);
        }else{
            decoder.Decode(null, 0);
        }
    }
    
    int DecodeLength(byte[] bytes)
    {
        int result = BitConverter.ToInt32(bytes, 0);
        return result;
    }

    byte[] EncodeLength(byte[] bytes, int length)
    {
        int[] lengthArr = new int[] { length };
        byte[] result = new byte[lengthArr.Length * sizeof(int)];
        Buffer.BlockCopy(lengthArr, 0, result, 0, result.Length);
        return AddByteToArray(bytes, result);
    }

    public byte[] AddByteToArray(byte[] bArray, byte[] newBytes)
    {
        byte[] newArray = new byte[bArray.Length + newBytes.Length];
        bArray.CopyTo(newArray, newBytes.Length);
        newBytes.CopyTo(newArray, 0);
        return newArray;
    }

    void EncodeAndSendBytes(byte[] data, int length)
    {
        if (audioConected)
        {
            if(data != null){
                audioBuffer = EncodeLength(data, length);
                audioDataChannel.Send(audioBuffer);
            }else{
                audioDataChannel.Send(new byte[1]);
            }
        }
    }
}
