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
// using Firebase.Extensions;
// using Newtonsoft.Json;
// using System.Threading.Tasks;

// enum PeerType {
//     Host,
//     Client
// }

// class PeerConnectionHOST : MonoBehaviour
// {
// #pragma warning disable 0649
//     [SerializeField] private PeerType peerType = PeerType.Host;
//     [SerializeField] private Button callButton;
//     [SerializeField] private Button restartButton;
//     [SerializeField] private Button hangUpButton;
//     [SerializeField] private Text localCandidateId;
//     [SerializeField] private Text remoteCandidateId;

//     [SerializeField] private Camera cam;
//     [SerializeField] private RawImage sourceImage;
//     [SerializeField] private RawImage receiveImage;

// #pragma warning restore 0649

//     private RTCPeerConnection _pc;
//     private List<RTCRtpSender> pc1Senders;
//     private MediaStream videoStream, receiveStream;
//     private DelegateOnIceConnectionChange pcOnIceConnectionChange;
//     private DelegateOnIceConnectionChange pc2OnIceConnectionChange;
//     private DelegateOnIceCandidate pcOnIceCandidate;
//     private DelegateOnIceCandidate pc2OnIceCandidate;
//     private DelegateOnTrack pcOntrack;
//     private DelegateOnNegotiationNeeded pcOnNegotiationNeeded;
//     private bool videoUpdateStarted;

//     private const int width = 720;
//     private const int height = 1280;

//     private DatabaseReference firebaseDB; //Todo: desacoplar BD

//     private void Awake()
//     {
//         WebRTC.Initialize(EncoderType.Software);
//         callButton.onClick.AddListener(Call);
//         restartButton.onClick.AddListener(RestartIce);
//         hangUpButton.onClick.AddListener(HangUp);
//         receiveStream = new MediaStream();
//     }

//     private void OnDestroy()
//     {
//         WebRTC.Dispose();
//     }

//     private void Start()
//     {
//         // Obtenemos referencia a la base de datos
//         firebaseDB = FirebaseDatabase.DefaultInstance.RootReference; // Todo: desacoplar BD

//         callButton.interactable = true;
//         restartButton.interactable = false;
//         hangUpButton.interactable = false;

//         // Configuramos metodos para los peers
//         if(peerType == PeerType.Host){
//             pc1Senders = new List<RTCRtpSender>();
//         }

//         pcOnIceConnectionChange = state => { OnIceConnectionChange(_pc, state); };
//         pcOnIceCandidate = candidate => { OnIceCandidate(_pc, candidate); };

//         // Si el peer es cliente asignamos el Track para el video
//         if(peerType == PeerType.Client){
//             pcOntrack = e =>
//             {
//                 receiveStream.AddTrack(e.Track);
//             };
//         }

//         //Aqui comienza la negociacion entre peers
//         pcOnNegotiationNeeded = () => { StartCoroutine(PeerNegotiationNeeded(_pc)); };

//         if(peerType == PeerType.Client){
//             receiveStream.OnAddTrack = e =>
//             {
//                 if (e.Track is VideoStreamTrack track)
//                 {
//                     receiveImage.texture = track.InitializeReceiver(width, height);
//                     receiveImage.color = Color.white;
//                 }
//             };
//             Debug.Log($"INTENTANDO CAPTURAR CAMARA");
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
            
//         sourceImage.texture = cam.targetTexture;
//         sourceImage.color = Color.white;
//     }

//     //Devuelve la configuracion del SDP
//     private static RTCConfiguration GetSelectedSdpSemantics()
//     {
//         RTCConfiguration config = default;
//         config.iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}};

//         return config;
//     }

//     // Devuelve informacion
//     private void OnIceConnectionChange(RTCPeerConnection pc, RTCIceConnectionState state)
//     {
//         Debug.Log($"{GetName()} IceConnectionState: {state}");

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

//         Debug.Log($"{GetName()} candidate stats Id:{remoteCandidateStats.Id}, Type:{remoteCandidateStats.candidateType}");
//     }
    
//     // El Host crea el sdp y comprueba si ha tenido exito
//     IEnumerator PeerNegotiationNeeded(RTCPeerConnection pc)
//     {
//         var op = pc.CreateOffer();
//         yield return op;

//         if (!op.IsError)
//         {
//             if (pc.SignalingState != RTCSignalingState.Stable)
//             {
//                 Debug.LogError($"{GetName()} signaling state is not stable.");
//                 yield break;
//             }

//             yield return StartCoroutine(OnCreateOfferSuccess(pc, op.Desc));
//         }
//         else
//         {
//             OnCreateSessionDescriptionError(op.Error);
//         }
//     }

//     // Añade los tracks de la transmision de video al host
//     private void AddTracks()
//     {
//         foreach (var track in videoStream.GetTracks())
//         {
//             pc1Senders.Add(_pc.AddTrack(track, videoStream));
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
//                 _pc.RemoveTrack(sender);
//             }

//             pc1Senders.Clear();
//         }
//         else if(peerType == PeerType.Client){
//             MediaStreamTrack[] tracks = receiveStream.GetTracks().ToArray();
//             foreach (var track in tracks)
//             {
//                 receiveStream.RemoveTrack(track);
//                 track.Dispose();
//             }
//         }
//     }

//     private void Call()
//     {
//         callButton.interactable = false;
//         hangUpButton.interactable = true;
//         restartButton.interactable = true;

//         var configuration = GetSelectedSdpSemantics();
//         _pc = new RTCPeerConnection(ref configuration);
//         _pc.OnIceCandidate = pcOnIceCandidate;
//         _pc.OnIceConnectionChange = pcOnIceConnectionChange;
//         _pc.OnNegotiationNeeded = pcOnNegotiationNeeded;

//         if(peerType == PeerType.Client){
//             _pc.OnTrack = pcOntrack;
//         }
//         else if(peerType == PeerType.Host){
//             AddTracks();
//         }   
//     }

//     private void RestartIce()
//     {
//         restartButton.interactable = false;

//         _pc.RestartIce();
//     }

//     private void HangUp()
//     {
//         RemoveTracks();

//         _pc.Close();
//         _pc.Dispose();
//         _pc = null;

//         callButton.interactable = true;
//         restartButton.interactable = false;
//         hangUpButton.interactable = false;

//         receiveImage.color = Color.black;
//     }

//     private void OnIceCandidate(RTCPeerConnection pc, RTCIceCandidate candidate)
//     {
//         //TODO: Enviar IceCandidate mediante Base de datos
//         GetOtherPc(pc).AddIceCandidate(candidate);
//         Debug.Log($"{GetName()} ICE candidate:\n {candidate.Candidate}");
//     }

//     private string GetName()
//     {
//         return (peerType == PeerType.Host) ? "Host" : "Client";
//     }


//     // TERMPORAL
//     public int GenerateRandomNo()
//     {
//         int _min = 1000;
//         int _max = 9999;
//         Random _rdm = new Random();
//         return _rdm.Next(_min, _max);
//     }


//     public async Task<Room> GetHostDescription(String RoomID){
//         Room receivedRoom = new Room();
//         var dataSnapshot = await firebaseDB.Child("rooms/"+RoomID).GetValueAsync();
//         receivedRoom = JsonConvert.DeserializeObject<Room>(dataSnapshot.GetRawJsonValue());
//         return receivedRoom;
//     }

//     // Establece el sdp local y lo transmite al otro peer
//     private IEnumerator OnCreateOfferSuccess(RTCPeerConnection pc, RTCSessionDescription descHost)
//     {
//         Debug.Log($"Offer from {GetName()}\n{descHost.sdp}");
//         Debug.Log($"{GetName()} setLocalDescription start");
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
//         string createdRoomID = GenerateRandomNo().ToString(); // Se muestra al usuario y el cliente lo recibe por un medio externo (whatsapp, etc..)

//         JObject roomJSON = JObject.FromObject(new Room{
//             Host = new User{
//                 Name = "Pepe",
//                 Description = descHost
//             },
//             Client = new User{
//                 Name = "Paco",
//                 Description = descHost
//             }
//         });

//         Debug.Log($"ENVIANDO A BASE DE DATOS:\n{roomJSON.ToString()}");
//         firebaseDB.Child("rooms/"+createdRoomID).SetRawJsonValueAsync(roomJSON.ToString());

// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ///////////////////////////////////////                        SIMULAMOS INTERNET AQUI                           //////////////////////////////////////////////
// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
       
//         var receivedRoomTask = GetHostDescription(createdRoomID);
//         yield return new WaitUntil(() => receivedRoomTask.IsCompleted);
//         Room receivedRoom = receivedRoomTask.Result;
//         Debug.Log($"RECIBIDO DE LA BASE DE DATOS:\n{JsonUtility.ToJson(receivedRoom)}");

//         var otherPc = GetOtherPc(pc);
//         Debug.Log($"{GetName(otherPc)} setRemoteDescription start");
//         var op2 = otherPc.SetRemoteDescription(ref receivedRoom.Host.Description);
//         yield return op2;
//         if (!op2.IsError)
//         {
//             OnSetRemoteSuccess(otherPc);
//         }
//         else
//         {
//             var error = op2.Error;
//             OnSetSessionDescriptionError(ref error);
//         }

//         Debug.Log($"{GetName(otherPc)} createAnswer start");
//         // Since the 'remote' side has no media stream we need
//         // to pass in the right constraints in order for it to
//         // accept the incoming offer of audio and video.

//         var op3 = otherPc.CreateAnswer();
//         yield return op3;
//         if (!op3.IsError)
//         {
//             yield return OnCreateAnswerSuccess(otherPc, op3.Desc);
//         }
//         else
//         {
//             OnCreateSessionDescriptionError(op3.Error);
//         }
//     }

//     private void OnSetLocalSuccess(RTCPeerConnection pc)
//     {
//         Debug.Log($"{GetName()} SetLocalDescription complete");
//     }

//     static void OnSetSessionDescriptionError(ref RTCError error)
//     {
//         Debug.LogError($"Error Detail Type: {error.message}");
//     }

//     private void OnSetRemoteSuccess(RTCPeerConnection pc)
//     {
//         Debug.Log($"{GetName()} SetRemoteDescription complete");
//     }

//     IEnumerator OnCreateAnswerSuccess(RTCPeerConnection pc, RTCSessionDescription descHost)
//     {
//         Debug.Log($"Answer from {GetName()}:\n{descHost.sdp}");
//         Debug.Log($"{GetName()} setLocalDescription start");
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

//         var otherPc = GetOtherPc(pc);
//         Debug.Log($"{GetName(otherPc)} setRemoteDescription start");


//         // AQUI ES DONDE REALIZAMOS EL SIGNALING //
//         var descClient = new RTCSessionDescription();
//         descClient.type = RTCSdpType.Answer; 
//         descClient.sdp = descHost.sdp; // Este valor es el que debemos compartir mediante firebase
//         // AQUI ES DONDE REALIZAMOS EL SIGNALING //


//         var op2 = otherPc.SetRemoteDescription(ref descClient);
//         yield return op2;
//         if (!op2.IsError)
//         {
//             OnSetRemoteSuccess(otherPc);
//         }
//         else
//         {
//             var error = op2.Error;
//             OnSetSessionDescriptionError(ref error);
//         }
//     }

//     private static void OnCreateSessionDescriptionError(RTCError error)
//     {
//         Debug.LogError($"Error Detail Type: {error.message}");
//     }
// }
