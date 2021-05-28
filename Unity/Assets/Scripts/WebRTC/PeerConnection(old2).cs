// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using Unity.WebRTC;
// using UnityEngine.UI;
// using Button = UnityEngine.UI.Button;
// using Firebase.Database;
// using Newtonsoft.Json.Linq;
// using Random = System.Random;
// using Newtonsoft.Json;
// using System.Threading.Tasks;

// class PeerConnection : MonoBehaviour
// {
// #pragma warning disable 0649
//     [SerializeField] private PeerType peerType = PeerType.Host;
//     [SerializeField] private Button callButton;
//     [SerializeField] private Button hangUpButton;
//     [SerializeField] private Camera cam;
//     [SerializeField] private RawImage videoImage;

// #pragma warning restore 0649

//     private RTCPeerConnection pc;
//     private List<RTCRtpSender> pc1Senders;
//     private MediaStream videoStream;
//     private DelegateOnIceConnectionChange pcOnIceConnectionChange;
//     private DelegateOnIceCandidate pcOnIceCandidate;
//     private DelegateOnTrack pcOntrack;
//     private DelegateOnNegotiationNeeded pcOnNegotiationNeeded;
//     private bool videoUpdateStarted;

//     private const int width = 720;
//     private const int height = 1280;

//     private DatabaseReference firebaseDB;
//     private String roomID;

//     private void Awake()
//     {
//         WebRTC.Initialize(EncoderType.Software);
//         callButton.onClick.AddListener(Call);
//         hangUpButton.onClick.AddListener(HangUp);
//         if(peerType == PeerType.Client){
//             videoStream = new MediaStream();
//         }
//     }

//     private void OnDestroy()
//     {
//         WebRTC.Dispose();
//     }

//     private void Start()
//     {
//         roomID = PersistentData.GetRoomID();
//         Debug.Log($"Room ID is {roomID}");

//         // Obtenemos referencia a la base de datos
//         firebaseDB = FirebaseDatabase.DefaultInstance.RootReference;
//         if(peerType == PeerType.Host){
//             FirebaseDatabase.DefaultInstance.GetReference($"rooms/{roomID}/Client/IceCandidates").ChildAdded += OnIceCandidateAdded;
//             FirebaseDatabase.DefaultInstance.GetReference($"rooms/{roomID}").ChildAdded += OnClientAdded;
//         }else{
//             FirebaseDatabase.DefaultInstance.GetReference($"rooms/{roomID}/Host/IceCandidates").ChildAdded += OnIceCandidateAdded;
//         }

//         // Configuramos metodos para los peers
//         if(peerType == PeerType.Host){
//             pc1Senders = new List<RTCRtpSender>();
//         }

//         callButton.interactable = true;
//         hangUpButton.interactable = false;

//         pcOnIceConnectionChange = state => { OnIceConnectionChange(pc, state); };
//         pcOnIceCandidate = candidate => { OnIceCandidate(pc, candidate); };

//         if(peerType == PeerType.Client){
//             pcOntrack = e =>
//             {
//                 videoStream.AddTrack(e.Track);
//             };
//         }

//         //Aqui comienza la negociacion entre peers
//         pcOnNegotiationNeeded = () => { StartCoroutine(HostPeerNegotiationNeeded(pc)); };

//         if(peerType == PeerType.Client){
//             videoStream.OnAddTrack = e =>
//             {
//                 if (e.Track is VideoStreamTrack track)
//                 {
//                     Debug.Log($"aqui se deberia empezar a transmitir {e.Track}");
//                     videoImage.texture = track.InitializeReceiver(width, height);
//                     videoImage.color = Color.white;
//                 }
//             };
//         }

//         if(peerType == PeerType.Host){
//             RecordCam();
//         }
 
//     }

//     // Capturamos la salida de la camara AR como textura para poder transmitirla.
//     private void RecordCam()
//     {
//         callButton.interactable = true;

//         if (videoStream == null)
//         {
//             videoStream = cam.CaptureStream(width, height, 1000000);
//         }
            
//         videoImage.texture = cam.targetTexture;
//         videoImage.color = Color.white;
//     }


//     //Devuelve la configuracion del SDP (offer)
//     private static RTCConfiguration GetSelectedSdpSemantics()
//     {
//         RTCConfiguration config = default;
//         config.iceServers = new[] {
//             new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}},
//             new RTCIceServer {urls = new[] {"stun:global.stun.twilio.com:3478?transport=udp"}},
//             new RTCIceServer {urls = new[] {"stun:stun.services.mozilla.com"}},
//             new RTCIceServer {urls = new[] {"turn:numb.viagenie.ca"}, username = "pablo.delosriosges@alum.uca.es", credential = "QCG%Q$x6XnzwNq"}
//             };

//         return config;
//     }

//     // Devuelve informacion
//     private void OnIceConnectionChange(RTCPeerConnection pc, RTCIceConnectionState state)
//     {
//         Debug.Log($"{peerType} IceConnectionState: {state}");

//         if (state == RTCIceConnectionState.Connected || state == RTCIceConnectionState.Completed)
//         {
//             StartCoroutine(CheckStats(pc));
//         }
//     }

//     // Display the video codec that is actually used.
//     IEnumerator CheckStats(RTCPeerConnection pc)
//     {
//         yield return new WaitForSeconds(0.1f);
//         if (pc == null)
//             yield break;

//         var op = pc.GetStats();
//         yield return op;
//         if (op.IsError)
//         {
//             Debug.LogErrorFormat("RTCPeerConnection.GetStats failed: {0}", op.Error);
//             yield break;
//         }

//         RTCStatsReport report = op.Value;
//         RTCIceCandidatePairStats activeCandidatePairStats = null;
//         RTCIceCandidateStats remoteCandidateStats = null;

//         foreach (var transportStatus in report.Stats.Values.OfType<RTCTransportStats>())
//         {
//             if (report.Stats.TryGetValue(transportStatus.selectedCandidatePairId, out var tmp))
//             {
//                 activeCandidatePairStats = tmp as RTCIceCandidatePairStats;
//             }
//         }

//         if (activeCandidatePairStats == null || string.IsNullOrEmpty(activeCandidatePairStats.remoteCandidateId))
//         {
//             yield break;
//         }

//         foreach (var iceCandidateStatus in report.Stats.Values.OfType<RTCIceCandidateStats>())
//         {
//             if (iceCandidateStatus.Id == activeCandidatePairStats.remoteCandidateId)
//             {
//                 remoteCandidateStats = iceCandidateStatus;
//             }
//         }

//         if (remoteCandidateStats == null || string.IsNullOrEmpty(remoteCandidateStats.Id))
//         {
//             yield break;
//         }

//         Debug.Log($"{peerType} candidate stats Id:{remoteCandidateStats.Id}, Type:{remoteCandidateStats.candidateType}");
//     }
    
//     // Crea el sdp y comprueba si ha tenido exito
//     IEnumerator HostPeerNegotiationNeeded(RTCPeerConnection pc)
//     {
//         var op = pc.CreateOffer();
//         yield return op;

//         if (!op.IsError)
//         {
//             if (pc.SignalingState != RTCSignalingState.Stable)
//             {
//                 Debug.LogError($"{peerType} signaling state is not stable.");
//                 yield break;
//             }

//             yield return StartCoroutine(OnCreateOfferSuccess(pc, op.Desc));
//         }
//         else
//         {
//             OnCreateSessionDescriptionError(op.Error);
//         }
//     }

//     private void AddTracks()
//     {
//         foreach (var track in videoStream.GetTracks())
//         {
//             pc1Senders.Add(pc.AddTrack(track, videoStream));
//         }

//         if (!videoUpdateStarted)
//         {
//             StartCoroutine(WebRTC.Update());
//             videoUpdateStarted = true;
//         }
//     }

//     private void RemoveTracks()
//     {
//         if(peerType == PeerType.Host){
//             foreach (var sender in pc1Senders)
//             {
//                 pc.RemoveTrack(sender);
//             }

//             pc1Senders.Clear();
//         }

//         if(peerType == PeerType.Client){
//             MediaStreamTrack[] tracks = videoStream.GetTracks().ToArray();
//             foreach (var track in tracks)
//             {
//                 videoStream.RemoveTrack(track);
//                 track.Dispose();
//             }
//         }
//     }

//     private void Call()
//     {
//         callButton.interactable = false;
//         hangUpButton.interactable = true;

//         var configuration = GetSelectedSdpSemantics();
//         pc = new RTCPeerConnection(ref configuration);
//         pc.OnIceCandidate = pcOnIceCandidate;
//         pc.OnIceConnectionChange = pcOnIceConnectionChange;
//         if(peerType == PeerType.Host){
//             pc.OnNegotiationNeeded = pcOnNegotiationNeeded;
//             AddTracks();    
//         }else{
//             StartCoroutine(ClientPeerNegotiationNeeded(pc));
//         }

//     }

//     private void HangUp()
//     {
//         RemoveTracks();

//         pc.Close();
//         pc.Dispose();
//         pc = null;

//         callButton.interactable = true;
//         hangUpButton.interactable = false;

//         if(peerType == PeerType.Client){
//             videoImage.color = Color.black;
//         }
//     }



//     private PeerType otherPeer(PeerType peertype){
//         return peertype == PeerType.Host ? PeerType.Client : PeerType.Host;
//     }




//     // Genera los datos necesarios para inicializar un ice candidate que mas tarde se guardaran en BD
//     private RTCIceCandidateInit InitIceCandidate(RTCIceCandidate candidate){
//         RTCIceCandidateInit iceCandidate = new RTCIceCandidateInit();
//         iceCandidate.candidate = candidate.Candidate;
//         iceCandidate.sdpMid = candidate.SdpMid;
//         iceCandidate.sdpMLineIndex = candidate.SdpMLineIndex;
//         return iceCandidate;
//     }

//     // Invocada cada vez que un peer encuentra un ice candidate
//     private async void OnIceCandidate(RTCPeerConnection pc, RTCIceCandidate candidate)
//     {
//         var candidateJSON = JObject.FromObject(InitIceCandidate(candidate));
//         Debug.Log($"{peerType} Enviando ICE candidate:\n {candidate.Candidate}");
//         if(peerType == PeerType.Host){
//             await firebaseDB.Child("rooms/"+roomID+"/Host/IceCandidates").Push().SetRawJsonValueAsync(candidateJSON.ToString());
//         }else{
//             await firebaseDB.Child("rooms/"+roomID+"/Client/IceCandidates").Push().SetRawJsonValueAsync(candidateJSON.ToString());
//         }

//     }

//     private void OnIceCandidateAdded(object sender, ChildChangedEventArgs args) {
//         if (args.DatabaseError != null) {
//             Debug.LogError(args.DatabaseError.Message);
//             return;
//         }
//         var candidateJSON = args.Snapshot.GetRawJsonValue();
//         var candidateInit = JsonConvert.DeserializeObject<RTCIceCandidateInit>(candidateJSON);
//         var candidate = new RTCIceCandidate(candidateInit);
//         pc.AddIceCandidate(candidate);
//         Debug.Log($"{peerType} Añadiendo ICE candidate:\n {candidate.Candidate}");
//     }

//     private void OnClientAdded(object sender, ChildChangedEventArgs args) {
//         if (args.DatabaseError != null) {
//             Debug.LogError(args.DatabaseError.Message);
//             return;
//         }
//         var clientJSON = args.Snapshot.GetRawJsonValue();
//         var client = JsonConvert.DeserializeObject<User>(clientJSON);
//         if(client.Description.type != RTCSdpType.Offer){
//             var receivedDesc = client.Description;
//             Debug.Log($"{peerType} setRemoteDescription start");

//             StartCoroutine(SetRemoteDescriptionAux(pc,receivedDesc));
//         }
//     }

//     private IEnumerator SetRemoteDescriptionAux(RTCPeerConnection pc, RTCSessionDescription receivedDesc){
//         Debug.Log($"{peerType} deberias ser host y esta deberia ser la respuesta: {receivedDesc.sdp} ");
//         var op2 = pc.SetRemoteDescription(ref receivedDesc);
//         yield return op2;
//         if (!op2.IsError)
//         {
//             OnSetRemoteSuccess(pc);
//         }
//         else
//         {
//             var error = op2.Error;
//             OnSetSessionDescriptionError(ref error);
//         }
//     }

//     // TERMPORAL
//     public int GenerateRandomNo()
//     {
//         int _min = 1000;
//         int _max = 9999;
//         Random _rdm = new Random();
//         return _rdm.Next(_min, _max);
//     }


//     public async Task<Room> GetRoom(String RoomID){
//         Room receivedRoom = new Room();
//         var dataSnapshot = await firebaseDB.Child("rooms/"+RoomID).GetValueAsync();
//         receivedRoom = JsonConvert.DeserializeObject<Room>(dataSnapshot.GetRawJsonValue());
//         return receivedRoom;
//     }

//     public async Task<RTCSessionDescription> GetSdp(String RoomID, String peer){
//         RTCSessionDescription receivedDesc = new RTCSessionDescription();
//         var dataSnapshot = await firebaseDB.Child("rooms/"+RoomID+"/"+peer+"/Description").GetValueAsync();
//         receivedDesc = JsonConvert.DeserializeObject<RTCSessionDescription>(dataSnapshot.GetRawJsonValue());
//         return receivedDesc;
//     }

//     // Establece el sdp local y lo transmite al otro peer
//     private IEnumerator OnCreateOfferSuccess(RTCPeerConnection pc, RTCSessionDescription descHost)
//     {
//         Debug.Log($"Offer from {peerType}\n{descHost.sdp}");
//         Debug.Log($"{peerType} setLocalDescription start");
//         var op = pc.SetLocalDescription(ref descHost);
//         yield return op;

//         if (!op.IsError)
//         {
//             OnSetLocalSuccess(pc);
//         }
//         else
//         {
//             var error = op.Error;
//             OnSetSessionDescriptionError(ref error);
//         }

//         // AQUI ES DONDE REALIZAMOS EL SIGNALING //
//         JObject roomJSON = JObject.FromObject(
//             new User{
//                 Name = "Pepe",
//                 Description = descHost
//             }
//         );
//         firebaseDB.Child("rooms/"+roomID+"/Host").SetRawJsonValueAsync(roomJSON.ToString());       
//     }

//     IEnumerator ClientPeerNegotiationNeeded(RTCPeerConnection pc){    
//         /// Todo esto se ejecuta cuando se escriba el host en base de datos
//         var receivedDescTask = GetSdp(roomID,"Host");
//         yield return new WaitUntil(() => receivedDescTask.IsCompleted);
//         RTCSessionDescription receivedDesc = receivedDescTask.Result;

//         Debug.Log($"{peerType} setRemoteDescription start");
//         var op2 = pc.SetRemoteDescription(ref receivedDesc);
//         yield return op2;
//         if (!op2.IsError)
//         {
//             OnSetRemoteSuccess(pc);
//         }
//         else
//         {
//             var error = op2.Error;
//             OnSetSessionDescriptionError(ref error);
//         }

//         Debug.Log($"{peerType} createAnswer start");
//         // Since the 'remote' side has no media stream we need
//         // to pass in the right constraints in order for it to
//         // accept the incoming offer of audio and video.

//         var op3 = pc.CreateAnswer();
//         yield return op3;
//         if (!op3.IsError)
//         {
//             yield return OnCreateAnswerSuccess(pc, op3.Desc);
//         }
//         else
//         {
//             OnCreateSessionDescriptionError(op3.Error);
//         }
//     }

//     private void OnSetLocalSuccess(RTCPeerConnection pc)
//     {
//         Debug.Log($"{peerType} SetLocalDescription complete");
//     }

//     static void OnSetSessionDescriptionError(ref RTCError error)
//     {
//         Debug.LogError($"Error Detail Type: {error.message}");
//     }

//     private void OnSetRemoteSuccess(RTCPeerConnection pc)
//     {
//         Debug.Log($"{peerType} SetRemoteDescription complete");
//     }

//     IEnumerator OnCreateAnswerSuccess(RTCPeerConnection pc, RTCSessionDescription descClient)
//     {
//         Debug.Log($"Answer from {peerType}:\n{descClient.sdp}");
//         Debug.Log($"{peerType} setLocalDescription start");
//         var op = pc.SetLocalDescription(ref descClient);
//         yield return op;

//         if (!op.IsError)
//         {
//             OnSetLocalSuccess(pc);
//         }
//         else
//         {
//             var error = op.Error;
//             OnSetSessionDescriptionError(ref error);
//         }


//         // AQUI ES DONDE REALIZAMOS EL SIGNALING //
//         JObject roomJSON = JObject.FromObject(
//             new User{
//                 Name = "Antonio",
//                 Description = descClient
//             }
//         );
//         firebaseDB.Child("rooms/"+roomID+"/Client").SetRawJsonValueAsync(roomJSON.ToString());
//     }

//     private static void OnCreateSessionDescriptionError(RTCError error)
//     {
//         Debug.LogError($"Error Detail Type: {error.message}");
//     }
// }
