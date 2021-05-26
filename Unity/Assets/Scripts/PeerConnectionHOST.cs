using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.WebRTC;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Firebase.Database;
using Newtonsoft.Json.Linq;
using Random = System.Random;
using Firebase.Extensions;
using Newtonsoft.Json;
using System.Threading.Tasks;

class PeerConnectionHOST : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button callButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button hangUpButton;
    [SerializeField] private Text localCandidateId;
    [SerializeField] private Text remoteCandidateId;

    [SerializeField] private Camera cam;
    [SerializeField] private RawImage sourceImage;
    [SerializeField] private RawImage receiveImage;

#pragma warning restore 0649

    private RTCPeerConnection _pc1, _pc2;
    private List<RTCRtpSender> pc1Senders;
    private MediaStream videoStream, receiveStream;
    private DelegateOnIceConnectionChange pc1OnIceConnectionChange;
    private DelegateOnIceConnectionChange pc2OnIceConnectionChange;
    private DelegateOnIceCandidate pc1OnIceCandidate;
    private DelegateOnIceCandidate pc2OnIceCandidate;
    private DelegateOnTrack pc2Ontrack;
    private DelegateOnNegotiationNeeded pc1OnNegotiationNeeded;
    private bool videoUpdateStarted;

    private const int width = 720;
    private const int height = 1280;

    private DatabaseReference firebaseDB;
    private String roomID = "1234";

    private void Awake()
    {
        WebRTC.Initialize(EncoderType.Software);
        callButton.onClick.AddListener(Call);
        restartButton.onClick.AddListener(RestartIce);
        hangUpButton.onClick.AddListener(HangUp);
        receiveStream = new MediaStream();
    }

    private void OnDestroy()
    {
        WebRTC.Dispose();
    }

    private void Start()
    {
        // Obtenemos referencia a la base de datos
        firebaseDB = FirebaseDatabase.DefaultInstance.RootReference;
        FirebaseDatabase.DefaultInstance.GetReference($"rooms/{roomID}/Host/IceCandidates").ChildAdded += OnHostIceCandidateAdded;
        FirebaseDatabase.DefaultInstance.GetReference($"rooms/{roomID}/Client/IceCandidates").ChildAdded += OnClientIceCandidateAdded;

        // Configuramos metodos para los peers
        pc1Senders = new List<RTCRtpSender>();
        callButton.interactable = true;
        restartButton.interactable = false;
        hangUpButton.interactable = false;

        pc1OnIceConnectionChange = state => { OnIceConnectionChange(_pc1, state); };
        pc2OnIceConnectionChange = state => { OnIceConnectionChange(_pc2, state); };
        pc1OnIceCandidate = candidate => { OnIceCandidate(_pc1, candidate); };
        pc2OnIceCandidate = candidate => { OnIceCandidate(_pc2, candidate); };
        pc2Ontrack = e =>
        {
            receiveStream.AddTrack(e.Track);
        };

        //Aqui comienza la negociacion entre peers
        pc1OnNegotiationNeeded = () => { StartCoroutine(PeerNegotiationNeeded(_pc1)); };

        receiveStream.OnAddTrack = e =>
        {
            if (e.Track is VideoStreamTrack track)
            {
                receiveImage.texture = track.InitializeReceiver(width, height);
                receiveImage.color = Color.white;
            }
        };
        Debug.Log($"INTENTANDO CAPTURAR CAMARA");

        RecordCam();
 
    }

    // Capturamos la salida de la camara AR como textura para poder transmitirla.
    private void RecordCam()
    {
        callButton.interactable = true;

        if (videoStream == null)
        {
            videoStream = cam.CaptureStream(width, height, 1000000);
        }
            
        sourceImage.texture = cam.targetTexture;
        sourceImage.color = Color.white;
    }


    //Devuelve la configuracion del SDP (offer)
    private static RTCConfiguration GetSelectedSdpSemantics()
    {
        RTCConfiguration config = default;
        config.iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}};

        return config;
    }

    // Devuelve informacion
    private void OnIceConnectionChange(RTCPeerConnection pc, RTCIceConnectionState state)
    {
        Debug.Log($"{GetName(pc)} IceConnectionState: {state}");

        if (state == RTCIceConnectionState.Connected || state == RTCIceConnectionState.Completed)
        {
            StartCoroutine(CheckStats(pc));
        }
    }

    // Display the video codec that is actually used.
    IEnumerator CheckStats(RTCPeerConnection pc)
    {
        yield return new WaitForSeconds(0.1f);
        if (pc == null)
            yield break;

        var op = pc.GetStats();
        yield return op;
        if (op.IsError)
        {
            Debug.LogErrorFormat("RTCPeerConnection.GetStats failed: {0}", op.Error);
            yield break;
        }

        RTCStatsReport report = op.Value;
        RTCIceCandidatePairStats activeCandidatePairStats = null;
        RTCIceCandidateStats remoteCandidateStats = null;

        foreach (var transportStatus in report.Stats.Values.OfType<RTCTransportStats>())
        {
            if (report.Stats.TryGetValue(transportStatus.selectedCandidatePairId, out var tmp))
            {
                activeCandidatePairStats = tmp as RTCIceCandidatePairStats;
            }
        }

        if (activeCandidatePairStats == null || string.IsNullOrEmpty(activeCandidatePairStats.remoteCandidateId))
        {
            yield break;
        }

        foreach (var iceCandidateStatus in report.Stats.Values.OfType<RTCIceCandidateStats>())
        {
            if (iceCandidateStatus.Id == activeCandidatePairStats.remoteCandidateId)
            {
                remoteCandidateStats = iceCandidateStatus;
            }
        }

        if (remoteCandidateStats == null || string.IsNullOrEmpty(remoteCandidateStats.Id))
        {
            yield break;
        }

        Debug.Log($"{GetName(pc)} candidate stats Id:{remoteCandidateStats.Id}, Type:{remoteCandidateStats.candidateType}");
        var updateText = GetName(pc) == "pc1" ? localCandidateId : remoteCandidateId;
        updateText.text = remoteCandidateStats.Id;
    }
    
    // Crea el sdp y comprueba siha tenido exito
    IEnumerator PeerNegotiationNeeded(RTCPeerConnection pc)
    {
        var op = pc.CreateOffer();
        yield return op;

        if (!op.IsError)
        {
            if (pc.SignalingState != RTCSignalingState.Stable)
            {
                Debug.LogError($"{GetName(pc)} signaling state is not stable.");
                yield break;
            }

            yield return StartCoroutine(OnCreateOfferSuccess(pc, op.Desc));
        }
        else
        {
            OnCreateSessionDescriptionError(op.Error);
        }
    }

    private void AddTracks()
    {
        foreach (var track in videoStream.GetTracks())
        {
            pc1Senders.Add(_pc1.AddTrack(track, videoStream));
        }

        if (!videoUpdateStarted)
        {
            StartCoroutine(WebRTC.Update());
            videoUpdateStarted = true;
        }
    }

    private void RemoveTracks()
    {
        foreach (var sender in pc1Senders)
        {
            _pc1.RemoveTrack(sender);
        }

        pc1Senders.Clear();

        MediaStreamTrack[] tracks = receiveStream.GetTracks().ToArray();
        foreach (var track in tracks)
        {
            receiveStream.RemoveTrack(track);
            track.Dispose();
        }
    }

    private void Call()
    {
        callButton.interactable = false;
        hangUpButton.interactable = true;
        restartButton.interactable = true;

        var configuration = GetSelectedSdpSemantics();
        _pc1 = new RTCPeerConnection(ref configuration);
        _pc1.OnIceCandidate = pc1OnIceCandidate;
        _pc1.OnIceConnectionChange = pc1OnIceConnectionChange;
        _pc1.OnNegotiationNeeded = pc1OnNegotiationNeeded;
        _pc2 = new RTCPeerConnection(ref configuration);
        _pc2.OnIceCandidate = pc2OnIceCandidate;
        _pc2.OnIceConnectionChange = pc2OnIceConnectionChange;
        _pc2.OnTrack = pc2Ontrack;

        AddTracks();
    }

    private void RestartIce()
    {
        restartButton.interactable = false;

        _pc1.RestartIce();
    }

    private void HangUp()
    {
        RemoveTracks();

        _pc1.Close();
        _pc2.Close();
        _pc1.Dispose();
        _pc2.Dispose();
        _pc1 = null;
        _pc2 = null;

        callButton.interactable = true;
        restartButton.interactable = false;
        hangUpButton.interactable = false;

        receiveImage.color = Color.black;
    }








    // Genera los datos necesarios para inicializar un ice candidate que mas tarde se guardaran en BD
    private RTCIceCandidateInit InitIceCandidate(RTCIceCandidate candidate){
        RTCIceCandidateInit iceCandidate = new RTCIceCandidateInit();
        iceCandidate.candidate = candidate.Candidate;
        iceCandidate.sdpMid = candidate.SdpMid;
        iceCandidate.sdpMLineIndex = candidate.SdpMLineIndex;
        return iceCandidate;
    }

    // Invocada cada vez que un peer encuentra un ice candidate
    private async void OnIceCandidate(RTCPeerConnection pc, RTCIceCandidate candidate)
    {
        String createdRoomID = "1234";
        String peer;
        if(pc == _pc1){
            peer = "Host";
        }
        else{
            peer = "Client";
        }

        var candidateJSON = JObject.FromObject(InitIceCandidate(candidate));
        Debug.Log($"ENVIANDO ICE CANDIDATE:\n{candidateJSON.ToString()}");
        await firebaseDB.Child("rooms/"+createdRoomID+"/"+peer+"/IceCandidates").Push().SetRawJsonValueAsync(candidateJSON.ToString());

    }

    private void OnHostIceCandidateAdded(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        var candidateJSON = args.Snapshot.GetRawJsonValue();
        var candidateInit = JsonConvert.DeserializeObject<RTCIceCandidateInit>(candidateJSON);
        var candidate = new RTCIceCandidate(candidateInit);
        _pc2.AddIceCandidate(candidate);
        Debug.Log($"pc2 ICE candidate:\n {candidate.Candidate}");
    }

    private void OnClientIceCandidateAdded(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        var candidateJSON = args.Snapshot.GetRawJsonValue();
        var candidateInit = JsonConvert.DeserializeObject<RTCIceCandidateInit>(candidateJSON);
        var candidate = new RTCIceCandidate(candidateInit);

        _pc1.AddIceCandidate(candidate);
        Debug.Log($"pc2 ICE candidate:\n {candidate.Candidate}");
    }










    private string GetName(RTCPeerConnection pc)
    {
        return (pc == _pc1) ? "pc1" : "pc2";
    }

    private RTCPeerConnection GetOtherPc(RTCPeerConnection pc)
    {
        return (pc == _pc1) ? _pc2 : _pc1;
    }

    // TERMPORAL
    public int GenerateRandomNo()
    {
        int _min = 1000;
        int _max = 9999;
        Random _rdm = new Random();
        return _rdm.Next(_min, _max);
    }


    public async Task<Room> GetRoom(String RoomID){
        Room receivedRoom = new Room();
        var dataSnapshot = await firebaseDB.Child("rooms/"+RoomID).GetValueAsync();
        receivedRoom = JsonConvert.DeserializeObject<Room>(dataSnapshot.GetRawJsonValue());
        return receivedRoom;
    }

    public async Task<RTCSessionDescription> GetSdp(String RoomID, String peer){
        RTCSessionDescription receivedDesc = new RTCSessionDescription();
        var dataSnapshot = await firebaseDB.Child("rooms/"+RoomID+"/"+peer+"/Description").GetValueAsync();
        receivedDesc = JsonConvert.DeserializeObject<RTCSessionDescription>(dataSnapshot.GetRawJsonValue());
        return receivedDesc;
    }

    // Establece el sdp local y lo transmite al otro peer
    private IEnumerator OnCreateOfferSuccess(RTCPeerConnection pc, RTCSessionDescription descHost)
    {
        Debug.Log($"Offer from {GetName(pc)}\n{descHost.sdp}");
        Debug.Log($"{GetName(pc)} setLocalDescription start");
        var op = pc.SetLocalDescription(ref descHost);
        yield return op;

        if (!op.IsError)
        {
            OnSetLocalSuccess(pc);
        }
        else
        {
            var error = op.Error;
            OnSetSessionDescriptionError(ref error);
        }

        // AQUI ES DONDE REALIZAMOS EL SIGNALING //
        string createdRoomID = "1234"; // Se muestra al usuario y el cliente lo recibe por un medio externo (whatsapp, etc..)

        JObject roomJSON = JObject.FromObject(
            new User{
                Name = "Pepe",
                Description = descHost
            }
        );

        Debug.Log($"ENVIANDO A BASE DE DATOS:\n{roomJSON.ToString()}");
        firebaseDB.Child("rooms/"+createdRoomID+"/Host").SetRawJsonValueAsync(roomJSON.ToString());

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////                        SIMULAMOS INTERNET AQUI                           //////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
       
        var receivedDescTask = GetSdp(createdRoomID,"Host");
        yield return new WaitUntil(() => receivedDescTask.IsCompleted);
        RTCSessionDescription receivedDesc = receivedDescTask.Result;
        Debug.Log($"RECIBIDO DE LA BASE DE DATOS=======================:\n{JsonUtility.ToJson(receivedDesc)}");

        var otherPc = GetOtherPc(pc);
        Debug.Log($"{GetName(otherPc)} setRemoteDescription start");
        var op2 = otherPc.SetRemoteDescription(ref receivedDesc);
        yield return op2;
        if (!op2.IsError)
        {
            OnSetRemoteSuccess(otherPc);
        }
        else
        {
            var error = op2.Error;
            OnSetSessionDescriptionError(ref error);
        }

        Debug.Log($"{GetName(otherPc)} createAnswer start");
        // Since the 'remote' side has no media stream we need
        // to pass in the right constraints in order for it to
        // accept the incoming offer of audio and video.

        var op3 = otherPc.CreateAnswer();
        yield return op3;
        if (!op3.IsError)
        {
            yield return OnCreateAnswerSuccess(otherPc, op3.Desc);
        }
        else
        {
            OnCreateSessionDescriptionError(op3.Error);
        }
    }

    private void OnSetLocalSuccess(RTCPeerConnection pc)
    {
        Debug.Log($"{GetName(pc)} SetLocalDescription complete");
    }

    static void OnSetSessionDescriptionError(ref RTCError error)
    {
        Debug.LogError($"Error Detail Type: {error.message}");
    }

    private void OnSetRemoteSuccess(RTCPeerConnection pc)
    {
        Debug.Log($"{GetName(pc)} SetRemoteDescription complete");
    }

    IEnumerator OnCreateAnswerSuccess(RTCPeerConnection pc, RTCSessionDescription descClient)
    {
        Debug.Log($"Answer from {GetName(pc)}:\n{descClient.sdp}");
        Debug.Log($"{GetName(pc)} setLocalDescription start");
        var op = pc.SetLocalDescription(ref descClient);
        yield return op;

        if (!op.IsError)
        {
            OnSetLocalSuccess(pc);
        }
        else
        {
            var error = op.Error;
            OnSetSessionDescriptionError(ref error);
        }


        // AQUI ES DONDE REALIZAMOS EL SIGNALING //
        string createdRoomID = "1234"; // Se muestra al usuario y el cliente lo recibe por un medio externo (whatsapp, etc..)

        JObject roomJSON = JObject.FromObject(
            new User{
                Name = "Antonio",
                Description = descClient
            }
        );

        Debug.Log($"ENVIANDO A BASE DE DATOS:\n{roomJSON.ToString()}");
        firebaseDB.Child("rooms/"+createdRoomID+"/Client").SetRawJsonValueAsync(roomJSON.ToString());

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////                        SIMULAMOS INTERNET AQUI                           //////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
       
        var receivedDescTask = GetSdp(createdRoomID,"Client");
        yield return new WaitUntil(() => receivedDescTask.IsCompleted);
        RTCSessionDescription receivedDesc = receivedDescTask.Result;
        Debug.Log($"RECIBIDO DE LA BASE DE DATOS=======================:\n{JsonUtility.ToJson(receivedDesc)}");

        var otherPc = GetOtherPc(pc);
        Debug.Log($"{GetName(otherPc)} setRemoteDescription start");

        var op2 = otherPc.SetRemoteDescription(ref receivedDesc);
        yield return op2;
        if (!op2.IsError)
        {
            OnSetRemoteSuccess(otherPc);
        }
        else
        {
            var error = op2.Error;
            OnSetSessionDescriptionError(ref error);
        }
    }

    private static void OnCreateSessionDescriptionError(RTCError error)
    {
        Debug.LogError($"Error Detail Type: {error.message}");
    }
}
