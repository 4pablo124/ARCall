using System;
using System.Linq;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.Android;

/// <summary>
/// Maneja la lógica de la captura y reproducción de audio
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// Evento lanzado cuando se el estado de la entrada de audio cambia (activo o inactivo)
    /// </summary>
    public event Action<bool> OnAudioInputEnable;
    /// <summary>
    /// Evento lanzado cuando la pista de audio esta lista
    /// </summary>
    public event Action OnAudioTrackReady;
    /// <summary>
    /// Indica si el audio esta listo
    /// </summary>
    [HideInInspector] public bool audioReady = false;
    /// <summary>
    /// Fuente de audio saliente (altavoz)
    /// </summary>
    [HideInInspector] public AudioSource outputAudioSource;
    /// <summary>
    /// Fuente de audio entrante (Micrófono)
    /// </summary>
    [HideInInspector] public AudioSource inputAudioSource;
    /// <summary>
    /// Tamaño del buffer
    /// </summary>
    [HideInInspector] public int bufferSize = 1024;
    /// <summary>
    /// Bitrate
    /// </summary>
    [HideInInspector] public ulong? bitrate = 20;
    private AudioStreamTrack audioStreamTrack;
    private int lengthSeconds = 1;
    private int samplingFreq = 16000;
    private AndroidJavaObject context;
    private AndroidJavaObject androidAudioManager;
    private int defaultMode;
    private bool defaultIsSpeakerphone;
    private int voiceMinVol;
    private int mediaMinVol;
    private int voiceMaxVol;
    private int mediaMaxVol;
    private int originalMediaVol;
    private PermissionCallbacks microphoneCallbacks;
    private bool muted;
    private string communicationDevice;
    private float[] spectrum;
    private int voiceVolume;
    private int lastCallVolume;

    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Inicializa los modelos referenciados y configura el audio de android</para>
    /// </summary>
    void Awake()
    {
        outputAudioSource = GameObject.Find("AudioOutput")?.GetComponent<AudioSource>();
        inputAudioSource = GameObject.Find("AudioInput")?.GetComponent<AudioSource>();

        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            androidAudioManager = context.Call<AndroidJavaObject>("getSystemService", "audio");

            defaultMode = androidAudioManager.Call<Int32>("getMode");
            defaultIsSpeakerphone = androidAudioManager.Call<Boolean>("isSpeakerphoneOn");

            voiceMinVol = 0;
            mediaMinVol = 1;
            voiceMaxVol = androidAudioManager.Call<int>("getStreamMaxVolume", 0);
            mediaMaxVol = androidAudioManager.Call<int>("getStreamMaxVolume", 3);
            originalMediaVol = androidAudioManager.Call<int>("getStreamVolume", 3);
        }

    }

    /// <summary>
    /// Alterna la entrada de audio
    /// </summary>
    /// <returns>El estado actual de la entrada de audio</returns>
    public bool ToggleMuteInput()
    {
        muted = !muted;
        inputAudioSource.mute = muted;
        OnAudioInputEnable?.Invoke(muted);
        return muted;
    }


    /// <summary>
    /// Alterna la salida de audio
    /// </summary>
    /// <returns>Estado actual de la salida de audio</returns>
    public bool ToggleMuteOutput()
    {
        muted = !muted;
        outputAudioSource.mute = muted;
        return muted;
    }

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Gestiona permisos de audio</para>
    /// </summary>
    private void Start()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            audioReady = true;
            OnAudioTrackReady?.Invoke();
        }
        else
        {
            microphoneCallbacks = new PermissionCallbacks();
            microphoneCallbacks.PermissionGranted += MicrophonePermissionGranted;
            microphoneCallbacks.PermissionDenied += MicrophonePermissionDenied;
            microphoneCallbacks.PermissionDeniedAndDontAskAgain += MicrophonePermissionDeniedAndDontAskAgain;
            Permission.RequestUserPermission(Permission.Microphone);
        }

        spectrum = new float[128];
    }

    /// <summary>
    /// Llamada al comienzo de cada fotograma
    /// <para>Gestiona volumen de la llamada</para>
    /// </summary>
    private void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            voiceVolume = androidAudioManager.Call<int>("getStreamVolume", 0);
            if(voiceVolume != lastCallVolume){
                androidAudioManager.Call("setStreamVolume",
                    3, // STREAM_MUSIC
                    (int)Math.Round(scaleValueToRange(voiceVolume, voiceMinVol, voiceMaxVol, mediaMinVol, mediaMaxVol)), // Usar el volumen de STREAM_VOICE_CALL
                    0 // no flags
                );
                lastCallVolume = voiceVolume;
            }
        }
    }

    /// <summary>
    /// Comienza a grabar el audio
    /// </summary>
    /// <returns>Pista de audio</returns>
    public AudioStreamTrack RecordAudio()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            // 0: MODE_NORMAL
            // 2: MODE_IN_CALL
            // 3: MODE_IN_COMMUNICATION
            SetVideocallAudio(3, true);
        }

        int minFreq;
        int maxFreq;

        // foreach (var device in Microphone.devices)
        // {
        //     Debug.Log(device);
        // }
        var device = Microphone.devices.SingleOrDefault(d => d == "Android audio input");
        Microphone.GetDeviceCaps(device, out minFreq, out maxFreq);
        var inputClip = Microphone.Start(device, true, lengthSeconds, (int)Mathf.Clamp(samplingFreq, minFreq, maxFreq));
        // set the latency to “0” samples before the audio starts to play.
        while (!(Microphone.GetPosition(null) > 0)) { }
        inputAudioSource.loop = true;
        inputAudioSource.clip = inputClip;
        inputAudioSource.Play();
        audioStreamTrack = new AudioStreamTrack(inputAudioSource);

        return audioStreamTrack;
    }

    /// <summary>
    /// Reproduce la salida de audio
    /// </summary>
    /// <param name="clip">Clip con el audio a reproducir</param>
    public void PlayAudio(AudioClip clip)
    {
        outputAudioSource.clip = clip;
        outputAudioSource.loop = true;
        outputAudioSource.Play();
    }

    /// <summary>
    /// Configura los ajustes de Android para videollamada
    /// </summary>
    /// <param name="mode">Modo de la llamdada</param>
    /// <param name="isSpeakerphoneOn">Si usar altavoces</param>
    private void SetVideocallAudio(int mode, bool isSpeakerphoneOn)
    {
        // androidAudioManager.Call()
        androidAudioManager.Call("setMode", mode);
        androidAudioManager.Call("setSpeakerphoneOn", isSpeakerphoneOn);
    }

    /// <summary>
    /// Escala el valor dado a un rango
    /// </summary>
    /// <param name="value">Valor de entrada</param>
    /// <param name="valueMin">Valor mínimo de la escala de entrada</param>
    /// <param name="valueMax">Valor máximo de la escala de entrada</param>
    /// <param name="rangeMin">Valor mínimo de la escala de salida</param>
    /// <param name="rangeMax">Valor máximo de la escala de salida</param>
    /// <returns></returns>
    private float scaleValueToRange(float value, float valueMin, float valueMax, float rangeMin, float rangeMax)
    {
        return ((value - valueMin) / (valueMax - valueMin)) * (rangeMax - rangeMin) + rangeMin;
    }

    //Permissions Callbacks

    /// <summary>
    /// Lanzada cuando se aceptan los permisos de audio
    /// </summary>
    /// <param name="permissionName">Nombre de los permisos aceptados</param>
    private void MicrophonePermissionGranted(string permissionName)
    {
        audioReady = true;
        OnAudioTrackReady?.Invoke();
    }
    /// <summary>
    /// Lanzada cuando se deniegan los permisos de audio
    /// </summary>
    /// <param name="permissionName">Nombre de los permisos denegados</param>    
    private void MicrophonePermissionDenied(string permissionName)
    {
        Permission.RequestUserPermission(Permission.Microphone);
    }
    /// <summary>
    /// Lanzada cuando se deniegan los permisos de audio y se indica que no se soliciten otra vez
    /// </summary>
    /// <param name="permissionName">Nombre de los permisos denegados</param>    
    private void MicrophonePermissionDeniedAndDontAskAgain(string permissionName)
    {
        MySceneManager.LoadScene("Main");
    }

    /// <summary>
    /// Llamada cuando el <see cref="GameObject"/> asociado de destruye
    /// <para>Devuelve la configuración a su estado anterior</para>
    /// </summary>
    private void OnDestroy()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            SetVideocallAudio(defaultMode, defaultIsSpeakerphone);
            androidAudioManager.Call("setStreamVolume", 3, originalMediaVol, 0);
        }

    }

    /// <summary>
    /// Devuelve el valor del volumen de una fuente de audio
    /// </summary>
    /// <param name="audioSource">Fuente de audio</param>
    /// <returns>Volumen</returns>
    public float GetVolume(AudioSource audioSource){
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Hamming);
        float sum = 0.0f;
        foreach (var sample in spectrum)
        {
            sum += sample * sample;
        }
        return Mathf.Sqrt(sum / spectrum.Length) * 1000;
    }
}
