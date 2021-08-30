using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Maneja la lógica de una conexión entre dos pares
/// </summary>
public class PeerConnectionManager : MonoBehaviour
{
    /// <summary>
    /// Evento lanzado cuando un usuario se conecta
    /// </summary>
    public event Action OnUserConnected;

    /// <summary>
    /// Rol del par actual
    /// </summary>
    public PeerType myPeerType = PeerType.Host;

    private MyInputManager inputManager;
    private VideoManager videoManager;
    private AudioManager audioManager;
    private ClientManager clientManager;
    private ARToolManager arToolManager;

    // private DatabaseReference database;
    private RTCDataChannel clientInputDataChannel, aspectRatioDataChannel, clientActionDataChannel;
    private RTCConfiguration RTCconfig;
    private RTCPeerConnection pc;
    private RTCRtpSender videoSender;
    private MediaStream mediaStream;
    private RTCRtpSender audioSender;
    // private readonly string[] excludeCodecMimeType = { "video/red", "video/ulpfec", "video/rtx" };


    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Inicializa los modelos referenciados y la configuración inicial de la conexión</para>
    /// </summary>
    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // myPeerType = peerType;
        inputManager = GameObject.Find("InputManager").GetComponent<MyInputManager>();
        videoManager = GameObject.Find("VideoManager").GetComponent<VideoManager>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        clientManager = GameObject.Find("ClientManager")?.GetComponent<ClientManager>();
        arToolManager = GameObject.Find("ARToolManager")?.GetComponent<ARToolManager>();

        //Establecemos ID de la sala
        GameObject.Find("RoomBtn").GetComponentInChildren<TextMeshProUGUI>().text = RoomManager.RoomID;

        // Inicializamos los callbacks
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // Host realiza llamada cuando llegue el cliente
        if (ImHost()) DatabaseManager.OnClientReady += OnClientReadyDelegate;

        // Cuando recibimos un mensaje invocamos de manera asincrona la lectura del mismo
        DatabaseManager.OnMessageReceived += OnMessageReceivedDelegate;

        audioManager.OnAudioInputEnable += enabled => EnableAudioStream(enabled);

        // var encoderType = WebRTC.SupportHardwareEncoder ? EncoderType.Hardware : EncoderType.Software;
        var encoderType = EncoderType.Software;
        WebRTC.Initialize(type: encoderType, limitTextureSize: true);
        StartCoroutine(WebRTC.Update());
    }

    /// <summary>
    /// Llamada cuando el cliente se conecta
    /// <para>Realiza la llamada</para>
    /// </summary>
    void OnClientReadyDelegate()
    {
        StartCoroutine(Call());
        pc.OnNegotiationNeeded = () => StartCoroutine(Call());
    }

    /// <summary>
    /// Llamada cuando se recibe un mensaje de base de datos
    /// <para>Delega la lectura del mensaje</para>
    /// </summary>
    /// <param name="msg">Mensaje</param>
    void OnMessageReceivedDelegate(Message msg)
    {
        StartCoroutine(ReadMessageDB(msg));
    }


    /// <summary>
    /// Llamada al comienzo de cada fotograma
    /// <para>Cierra el canal de audio cuando este no tiene actividad o esta deshabilitado
    /// </summary>
    private void Update() {
        if(audioSender.Track.Enabled && audioManager.GetVolume(audioManager.inputAudioSource) < 1){     
            EnableAudioStream(false);
        }else

        if(!audioSender.Track.Enabled && audioManager.GetVolume(audioManager.inputAudioSource) >= 1){     
            EnableAudioStream(true);
        }
    }
 
    /// <summary>
    /// Cambia el estado del canal de audio
    /// </summary>
    /// <param name="enabled">Estado</param>
    private void EnableAudioStream(bool enabled){
        audioSender.Track.Enabled = enabled;
    }

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Comienza el proceso de conexión</para>
    /// </summary>
    private async void Start()
    {
        // Señalizamos que el peer esta listo
        await DatabaseManager.ReadyUser(RoomManager.RoomID, myPeerType);

        // Configuramos servidores ICE
        RTCconfig = ServersConfig();

        // Creamos nuestro peer
        pc = new RTCPeerConnection(ref RTCconfig)
        {
            // Cuando se encuentre un Ice Candidate
            OnIceCandidate = candidate =>
            {
                DatabaseManager.SendMessage(RoomManager.RoomID, myPeerType, new Data { ice = InitIceCandidate(candidate) });
                Debug.Log($"{myPeerType} - Ice Enviado: {candidate.Candidate}");
            },

            // Cuando cambie el estado de la conexion
            OnIceConnectionChange = state =>
            {
                Debug.Log($"{myPeerType} - IceConnectionState: {state}");
                switch (state)
                {
                    case RTCIceConnectionState.Disconnected:
                        OnDisconnect();
                        break;
                    case RTCIceConnectionState.Connected:
                        OnUserConnected?.Invoke();
                        if(ImHost()) SetVideoParameters();
                        SetAudioParameters();
                        break;
                    case RTCIceConnectionState.Failed:
                        if (myPeerType == PeerType.Host) StartCoroutine(Call());
                        break;
                }
            }
        };

        AddDataChannels();
        AddTracks();

    }

    /// <summary>
    /// Establece los paramentros de la trasnmisión de video
    /// </summary>
    /// <returns>Si se han establecido los parametros correctamente</returns>
    private bool SetVideoParameters(){
        var videoParameters = videoSender.GetParameters();
        foreach (var encoding in videoParameters.encodings)
        {
            encoding.minBitrate = videoManager.bitrate * 1000;
            encoding.maxBitrate = videoManager.bitrate * 1000;
            encoding.maxFramerate = videoManager.framerate;
            encoding.scaleResolutionDownBy = 1.5;
            encoding.active=true;
        }
        videoSender.SetParameters(videoParameters);
        RTCErrorType videoError = videoSender.SetParameters(videoParameters);
        if (videoError != RTCErrorType.None)
        {
            Debug.LogErrorFormat("RTCRtpSender.SetParameters failed {0}", videoError);
        }

        return videoParameters.encodings.Length > 0;
    }

    /// <summary>
    /// Establece los paramentros de la trasnmisión de audio
    /// </summary>
    /// <returns>Si se han establecido los parametros correctamente</returns>
    private bool SetAudioParameters(){
        var audioParameters = audioSender.GetParameters();
        foreach (var encoding in audioParameters.encodings)
        {
            encoding.minBitrate = audioManager.bitrate * 1000;
            encoding.maxBitrate = audioManager.bitrate * 1000;
        }
        audioSender.SetParameters(audioParameters);
        RTCErrorType audioError = audioSender.SetParameters(audioParameters);
        if (audioError != RTCErrorType.None)
        {
            Debug.LogErrorFormat("RTCRtpSender.SetParameters failed {0}", audioError);
        }

        return audioParameters.encodings.Length > 0;
    }

    // MAIN CONECCTION
    /// <summary>
    /// Crea los canales de video y audio
    /// </summary>
    private void AddTracks()
    {
        //ENVIAMOS
        mediaStream = new MediaStream();

        // video
        if (ImHost())
        {
            videoSender = pc.AddTrack(videoManager.RecordCamera(), mediaStream);
        }

        // audio
        if (audioManager.audioReady)
        {
            audioSender = pc.AddTrack(audioManager.RecordAudio(), mediaStream);
        }
        else
        {
            audioManager.OnAudioTrackReady += () =>
            {
                audioSender = pc.AddTrack(audioManager.RecordAudio(), mediaStream);
            };
        }
        

        //RECIBIMOS
        mediaStream.OnAddTrack = e =>
        {
            // video
            if (e.Track is VideoStreamTrack videoTrack)
            {
                videoManager.videoRawImage.texture = videoTrack.InitializeReceiver(videoManager.width, videoManager.height);
                videoManager.videoRawImage.texture.filterMode = FilterMode.Trilinear;
                videoManager.videoRawImage.color = Color.white;
            }
            // audio
            else if (e.Track is AudioStreamTrack audioTrack)
            {
                audioTrack.OnAudioReceived += clip => audioManager.PlayAudio(clip);
            }
        };
        pc.OnTrack = e => mediaStream.AddTrack(e.Track);

    }

    /// <summary>
    /// Crea los canales de datos
    /// </summary>
    private void AddDataChannels()
    {

        // Creamos canales de Datos
        RTCDataChannelInit conf = new RTCDataChannelInit() { ordered = true };

        if (ImHost())
        {
            // Send
            aspectRatioDataChannel = pc.CreateDataChannel("aspectRatio", conf);
            aspectRatioDataChannel.OnOpen = async() =>
            {
                await Task.Delay(1000);
                aspectRatioDataChannel.Send(BitConverter.GetBytes(videoManager.aspectRatio));
            };

            // Receive
            pc.OnDataChannel = channel =>
            {
                switch (channel.Label)
                {
                    case "clientInput":
                        clientInputDataChannel = channel;
                        clientInputDataChannel.OnMessage = bytes =>
                        {
                            var inputJson = System.Text.Encoding.UTF8.GetString(bytes);
                            inputManager.clientPosition = JsonUtility.FromJson<Vector3>(inputJson);
                        };
                        break;

                    case "clientAction":
                        clientActionDataChannel = channel;
                        clientActionDataChannel.OnMessage = bytes =>
                        {
                            var msg = System.Text.Encoding.UTF8.GetString(bytes);
                            switch (msg)
                            {
                                case "undo": arToolManager.UndoDrawing("Client"); break;
                                case "deleteClient": arToolManager.DeleteDrawings("Client"); break;
                                case "deleteBoth": arToolManager.DeleteDrawings("Both"); break;
                                case "ARBrush":
                                case "ARPointer":
                                case "ARMarker":
                                case "ARText":
                                    arToolManager.SelectTool(PeerType.Client, msg);
                                    break;
                                case "DC6B6D":
                                case "6BDC99":
                                case "6BD4DC":
                                case "FFF64A":
                                    arToolManager.SelectColor(PeerType.Client, msg);
                                    break;
                            }
                        };
                        break;
                }
            };
        }
        else

        if (!ImHost())
        {
            // Send
            clientInputDataChannel = pc.CreateDataChannel("clientInput", conf);
            inputManager.OnClientInput += inputJson =>
            {
                if (clientInputDataChannel.ReadyState == RTCDataChannelState.Open)
                {
                    clientInputDataChannel.Send(inputJson);
                }
            };

            clientActionDataChannel = pc.CreateDataChannel("clientAction", conf);
            clientManager.OnToolSelected += tool =>
            {
                if (clientActionDataChannel.ReadyState == RTCDataChannelState.Open)
                {
                    clientActionDataChannel.Send(tool);
                }
            };
            clientManager.OnUndo += () =>
            {
                if (clientActionDataChannel.ReadyState == RTCDataChannelState.Open)
                {
                    clientActionDataChannel.Send("undo");
                }
            };
            clientManager.OnDelete += peer =>
            {
                if (clientActionDataChannel.ReadyState == RTCDataChannelState.Open)
                {
                    clientActionDataChannel.Send("delete" + peer);
                }
            };
            clientManager.OnColorSelected += color =>
            {
                if (clientActionDataChannel.ReadyState == RTCDataChannelState.Open)
                {
                    clientActionDataChannel.Send(color);
                }
            };

            // Receive
            pc.OnDataChannel = channel =>
            {
                switch (channel.Label)
                {
                    case "aspectRatio":
                        aspectRatioDataChannel = channel;
                        aspectRatioDataChannel.OnMessage = bytes =>
                        {
                            videoManager.aspectRatio = BitConverter.ToSingle(bytes, 0);

                            videoManager.height = (int)Math.Round(videoManager.width / videoManager.aspectRatio);
                            videoManager.videoRawImage.GetComponent<AspectRatioFitter>().aspectRatio = videoManager.aspectRatio;
                        };
                        break;
                }
            };
        }
    }

    /// <summary>
    /// Realiza la llamada para establecer la conexión
    /// </summary>
    /// <returns>Corutina de Unity</returns>
    private IEnumerator Call()
    {

        Debug.Log($"{myPeerType} - Realizando conexion");

        var op = pc.CreateOffer();
        yield return op;
        Debug.Log($"{myPeerType} - CreateOffer: {!op.IsError}");
        if (op.IsError) Debug.Log($"{myPeerType} - CreateOffer [Error]: {op.Error.message}");

        var offer = op.Desc;
        var op2 = pc.SetLocalDescription(ref offer);
        yield return op2;
        Debug.Log($"{myPeerType} - SetLocalDescription: {!op2.IsError}");
        if (op2.IsError) Debug.Log($"{myPeerType} - SetLocalDescription [Error]: {op2.Error.message}");

        Debug.Log($"{myPeerType} - Sending Offer");
        DatabaseManager.SendMessage(RoomManager.RoomID, myPeerType, new Data { sdp = pc.LocalDescription });

    }





    // CALLBACKS
    /// <summary>
    /// Llamada cuando se abandona la escena
    /// <para>Indica que el par no esta disponible en base de datos</para>
    /// </summary>
    /// <typeparam name="Scene">Escena de Unity</typeparam>
    /// <param name="scene">Escena que se ha abandonado</param>
    private void OnSceneUnloaded<Scene>(Scene scene)
    {
        UnReadyUser();
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    /// <summary>
    /// Llamada cuando el estado de pausa de la aplicación cambia
    /// <para>Alterna el estado de listo del par en la conexión</para>
    /// </summary>
    /// <param name="paused">Estado de pausa</param>
    private void OnApplicationPause(bool paused)
    {
        if (paused) { UnReadyUser(); }
        else { ReadyUser(); }
    }

    /// <summary>
    /// Llamada cuando el estado de efonque de la aplicación cambia
    /// <para>Alterna el estado de listo del par en la conexión</para>
    /// </summary>
    /// <param name="focused">Estado de enfoque</param>
    private void OnApplicationFocus(bool focused)
    {
        if (focused) { ReadyUser(); }
        else { UnReadyUser(); }
    }

    /// <summary>
    /// Llamada cuando el <see cref="GameObject"/> asociado se destruye
    /// <para>Finaliza la conexión</para>
    /// </summary>
    private void OnDestroy()
    {
        if (ImHost()) DatabaseManager.OnClientReady -= OnClientReadyDelegate;
        DatabaseManager.OnMessageReceived -= OnMessageReceivedDelegate;

        WebRTC.Dispose();
        Screen.sleepTimeout = SleepTimeout.SystemSetting;

    }

    /// <summary>
    /// Llamada cuando la conexión se pierde
    /// <para>Se cuelga la videollamda</para>
    /// </summary>
    private void OnDisconnect()
    {
        HangUp();
    }

    /// <summary>
    /// Cuelga la videollamada
    /// </summary>
    public void HangUp()
    {
        if (myPeerType == PeerType.Host)
        {
            videoManager.arSession.Reset();
        }

        UnReadyUser();

        pc.Close();
        pc.Dispose();

#if UNITY_ANDROID || UNITY_EDITOR
        MySceneManager.LoadScene("Main");
#else
            MySceneManager.LoadScene("JoinRoom");
#endif
    }


    /// <summary>
    /// Lee un mensaje mediante la base de datos, se invoca cada vez que se envie un mensaje
    /// </summary>
    /// <param name="msg">Mensaje</param>
    /// <returns>Corutina de Unity</returns>
    private IEnumerator ReadMessageDB(Message msg)
    {
        //El mensaje es para mi
        if (myPeerType != msg.peerType)
        {

            // Me envia Ice
            if (msg.data.ice != null)
            {
                pc.AddIceCandidate(new RTCIceCandidate(msg.data.ice));
                Debug.Log($"{myPeerType} - AddIceCandidate: {msg.data.ice.candidate}");
            }

            // Me envia Sdp (offer)
            else if (msg.data.sdp.type == RTCSdpType.Offer)
            {
                // Guardamos offer
                var description = msg.data.sdp;
                var op = pc.SetRemoteDescription(ref description);
                yield return op;
                Debug.Log($"{myPeerType} - SetRemoteDescription: {!op.IsError}");

                // Creamos Answer
                var op2 = pc.CreateAnswer();
                yield return op2;
                Debug.Log($"{myPeerType} - CreateAnswer: {!op2.IsError}");


                // La guardamos
                var answer = op2.Desc;
                var op3 = pc.SetLocalDescription(ref answer);
                yield return op3;
                Debug.Log($"{myPeerType} - SetLocalDescription: {!op3.IsError}");

                // Y la enviamos
                Debug.Log($"{myPeerType} - Sending Answer");
                DatabaseManager.SendMessage(RoomManager.RoomID, myPeerType, new Data { sdp = pc.LocalDescription });
            }

            // Me envia Sdp (Answer)
            else if (msg.data.sdp.type == RTCSdpType.Answer)
            {
                var answer = msg.data.sdp;
                var op4 = pc.SetRemoteDescription(ref answer);
                yield return op4;
                Debug.Log($"{myPeerType} - SetRemoteDescription: {!op4.IsError}");
                if (op4.IsError)
                {
                    Debug.Log(op4.Error.message);
                }

            }
        }

    }

    /// <summary>
    /// Señaliza que el par no listo para la conexión
    /// </summary>
    private async void UnReadyUser()
    {
        // Quitamos al peer de la base de datos, se eliminara la sala automaticamente cuando no haya peers
        await DatabaseManager.UnReadyUser(RoomManager.RoomID, myPeerType);
        // Devolvemos la pantalla a su estado original
    }

    /// <summary>
    /// Señaliza que el par esta listo para la conexión
    /// </summary>
    private async void ReadyUser()
    {
        await DatabaseManager.ReadyUser(RoomManager.RoomID, myPeerType);
    }


    // CONNECTION AUX

    /// <summary>
    /// Genera la configuracion de servidores ICE y protocolos de transmisión
    /// </summary>
    /// <returns>Configuración de servidores</returns>
    private static RTCConfiguration ServersConfig()
    {
        RTCConfiguration config = default;
        config.iceServers = new[] {
            new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}},
            new RTCIceServer {urls = new[] {"stun:stun1.l.google.com:19302"}},
            new RTCIceServer {urls = new[] {"stun:stun2.l.google.com:19302"}},
            new RTCIceServer {urls = new[] {"stun:stun3.l.google.com:19302"}},
            new RTCIceServer {urls = new[] {"stun:stun4.l.google.com:19302"}},
            new RTCIceServer {urls = new[] {"stun:stun.ekiga.net"}},
            new RTCIceServer {urls = new[] {"stun:stun.ideasip.com"}},
            new RTCIceServer {urls = new[] {"stun:stun.rixtelecom.se"}},
            new RTCIceServer {urls = new[] {"stun:stun.schlund.de"}},
            new RTCIceServer {urls = new[] {"stun:stun.stunprotocol.org:3478"}},
            new RTCIceServer {urls = new[] {"stun:stun.voiparound.com"}},
            new RTCIceServer {urls = new[] {"stun:stun.voipbuster.com"}},
            new RTCIceServer {urls = new[] {"stun:stun.voipstunt.com"}},
            new RTCIceServer {urls = new[] {"stun:stun.voxgratia.org"}},
            new RTCIceServer {urls = new[] {"stun:global.stun.twilio.com:3478?transport=udp"}},
            new RTCIceServer {urls = new[] {"stun:stun.services.mozilla.com"}},
            new RTCIceServer {urls = new[] {"turn:numb.viagenie.ca"}, username = "pablo.delosriosges@alum.uca.es", credential = "QCG%Q$x6XnzwNq"},
            new RTCIceServer {urls = new[] {"turn:192.158.29.39:3478?transport=udp"}, username = "28224511:1379330808", credential = "JZEOEt2V3Qb0y27GRntt2u2PAYA="},
            new RTCIceServer {urls = new[] {"turn:192.158.29.39:3478?transport=tcp"}, username = "28224511:1379330808", credential = "JZEOEt2V3Qb0y27GRntt2u2PAYA="},
            new RTCIceServer {urls = new[] {"turn:turn.bistri.com:80"}, username = "homeo", credential = "homeo"},
            new RTCIceServer {urls = new[] {"turn:turn.anyfirewall.com:443?transport=tcp"}, username = "webrtc", credential = "webrtc"}
            };

        return config;
    }

    /// <summary>
    /// Genera la informacion necesaria para inicializar un <see cref="RTCIceCandidate"/>
    /// </summary>
    /// <param name="candidate">ICE candidate</param>
    /// <returns></returns>
    private RTCIceCandidateInit InitIceCandidate(RTCIceCandidate candidate)
    {
        RTCIceCandidateInit iceCandidate = new RTCIceCandidateInit()
        {
            candidate = candidate.Candidate,
            sdpMid = candidate.SdpMid,
            sdpMLineIndex = candidate.SdpMLineIndex
        };
        return iceCandidate;
    }


    // MISC AUX
    /// <summary>
    /// Evalua si el par actual es host
    /// </summary>
    /// <returns>Si el par actual es host</returns>
    private bool ImHost()
    {
        return myPeerType == PeerType.Host;
    }
}
