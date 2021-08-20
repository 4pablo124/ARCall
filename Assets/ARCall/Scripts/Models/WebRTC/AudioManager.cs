using System;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.Android;

public class AudioManager : MonoBehaviour
{
    public event Action OnAudioTrackReady;
    [HideInInspector] public bool audioReady = false;
    [HideInInspector] public AudioSource outputAudioSource;
    [HideInInspector] public AudioSource inputAudioSource;
    [HideInInspector] public int bufferSize = 1024;
    [HideInInspector] public ulong? bitrate = 40;
    private AudioStreamTrack audioStreamTrack;
    private int lengthSeconds = 1;
    private int samplingFreq = 48000;
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

    public bool ToggleMute(AudioSource audioSource)
    {
        if (muted)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
        return muted = !muted;
    }

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

        var audioConf = AudioSettings.GetConfiguration();
        audioConf.dspBufferSize = bufferSize;
    }

    private void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            androidAudioManager.Call("setStreamVolume",
                3, // STREAM_MUSIC
                (int)Math.Round(scaleValueToRange(androidAudioManager.Call<int>("getStreamVolume", 0), voiceMinVol, voiceMaxVol, mediaMinVol, mediaMaxVol)), // Usar el volumen de STREAM_VOICE_CALL
                0 // no flags
            );
        }
    }

    public AudioStreamTrack RecordAudio()
    {
        foreach (string device in Microphone.devices)
        {
            Debug.Log(device);
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            // 0: MODE_NORMAL
            // 2: MODE_IN_CALL
            // 3: MODE_IN_COMMUNICATION
            SetVideocallAudio(3, true);
        }

        int minFreq;
        int maxFreq;
        Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);

        var inputClip = Microphone.Start(null, true, lengthSeconds, (int)Mathf.Clamp(samplingFreq, minFreq, maxFreq));
        // set the latency to “0” samples before the audio starts to play.
        while (!(Microphone.GetPosition(null) > 0)) { }
        inputAudioSource.loop = true;
        inputAudioSource.clip = inputClip;
        inputAudioSource.Play();
        audioStreamTrack = new AudioStreamTrack(inputAudioSource);

        return audioStreamTrack;
    }

    public void PlayAudio(AudioClip clip)
    {
        Debug.Log("Playing audio");
        outputAudioSource.clip = clip;
        outputAudioSource.loop = true;
        outputAudioSource.Play();
    }

    private void SetVideocallAudio(int mode, bool isSpeakerphoneOn)
    {
        androidAudioManager.Call("setMode", mode);
        androidAudioManager.Call("setSpeakerphoneOn", isSpeakerphoneOn);
    }

    private float scaleValueToRange(float value, float valueMin, float valueMax, float rangeMin, float rangeMax)
    {
        return ((value - valueMin) / (valueMax - valueMin)) * (rangeMax - rangeMin) + rangeMin;
    }

    //Permissions Callbacks
    private void MicrophonePermissionGranted(string permissionName)
    {
        audioReady = true;
        OnAudioTrackReady?.Invoke();
    }
    private void MicrophonePermissionDenied(string permissionName)
    {
        Permission.RequestUserPermission(Permission.Microphone);
    }
    private void MicrophonePermissionDeniedAndDontAskAgain(string permissionName)
    {
        MySceneManager.LoadScene("Main");
    }

    private void OnDestroy()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            SetVideocallAudio(defaultMode, defaultIsSpeakerphone);
            androidAudioManager.Call("setStreamVolume", 3, originalMediaVol, 0);
        }

    }
}
