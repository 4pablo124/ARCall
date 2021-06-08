using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using Unity.WebRTC;
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

    private String roomID;
    private DatabaseReference database;
    private MediaStream videoStream;
    private MediaStream audioStream;
    private RTCDataChannel audioDataChannel, remoteAudioDataChannel;
    private RTCConfiguration RTCconfig;
    private RTCPeerConnection pc;
    private List<RTCRtpSender> pcSenders;

    private bool videoUpdateStarted = false;
    private byte[] audioBuffer;
    private bool audioConected = false;
    private bool showingVideo = true;
    private const int width = 360;
    private const int height = 640;

    private void Awake()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // Evitamos que la pantalla se apague
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
     
        // Inicializamos WebRTC con decodificacion por software, ya que da problemas en android
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

    private void OnDestroy()
    {
        // Desarmamos webRTC en destructor
        Debug.Log("WebRTC.Dispose()");
        WebRTC.Dispose();
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
        pc.OnIceCandidate = candidate => {OnIceCandidate(candidate);};
        pc.OnIceConnectionChange = state => { Debug.Log($"{myPeerType} - IceConnectionState: {state}"); };

        //pc.GetSenders().First().SetParameters(SetBandwidth(pc.GetSenders().First().GetParameters(), 1000000, 125000));

        if(ImHost()){ 
            videoStream = VideoManager.RecordLiveVideo(ref cam, ref videoImage);
        }

        // Añadimos los diferentes tracks
        AddTracks();

    }

    // private RTCRtpSendParameters SetBandwidth(RTCRtpSendParameters parameters, ulong maxBitrate, ulong minBitrate){
    //     parameters.encodings[0].maxBitrate = maxBitrate;
    //     parameters.encodings[0].minBitrate = minBitrate;
    //     return parameters;
    // }

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

    //Añade los tracks a la conexion
    private void AddTracks()
    {   
        if(ImHost()){
            // Enviamos video
            foreach (var track in videoStream.GetTracks())
            {
                pcSenders.Add(pc.AddTrack(track, videoStream));
            }
        }
        else if(!ImHost()){
            // Recibimos video
            pc.OnTrack = e => videoStream.AddTrack(e.Track);    
            videoStream = new MediaStream();
            videoStream.OnAddTrack = e => {
                if (e.Track is VideoStreamTrack track)
                {
                    videoImage.texture = track.InitializeReceiver(width, height);
                    videoImage.color = Color.white;
                }
            };
        }

        // Enviamos audioData
        RTCDataChannelInit conf = new RTCDataChannelInit();
        audioDataChannel = pc.CreateDataChannel("audio", conf);
        encoder.OnEncoded += EncodeAndSendBytes;

        // Recibimos data (y audio gracias a UnityOpus)
        pc.OnDataChannel = channel =>{
            remoteAudioDataChannel = channel;
            remoteAudioDataChannel.OnMessage = bytes => { Decode(bytes); };
        };
        //PROVISIONAL
        audioDataChannel.OnOpen = () => { audioConected = true; };
        audioDataChannel.OnClose = () => { audioConected = false; };

        // Inciamos actualizacion de "frames"
        if (!videoUpdateStarted)
        {
            Debug.Log("WebRTC.Update() Started");
            StartCoroutine(WebRTC.Update());
            videoUpdateStarted = true;
        }
    }


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

    public void shareRoom(){
        RoomManager.shareRoom(roomID);
    }

    public bool ImHost(){
        return myPeerType == PeerType.Host;
    }
}
