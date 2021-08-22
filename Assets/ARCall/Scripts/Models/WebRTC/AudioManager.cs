using System;
using System.Linq;
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

    public event Action<bool> OnAudioInputEnable;

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

    public bool ToggleMuteInput()
    {
        muted = !muted;
        inputAudioSource.mute = muted;
        OnAudioInputEnable?.Invoke(muted);
        return muted;
    }

    public bool ToggleMuteOutput()
    {
        muted = !muted;
        outputAudioSource.mute = muted;
        return muted;
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

        spectrum = new float[128];
    }

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

    public void PlayAudio(AudioClip clip)
    {
        outputAudioSource.clip = clip;
        outputAudioSource.loop = true;
        outputAudioSource.Play();
    }

    private void SetVideocallAudio(int mode, bool isSpeakerphoneOn)
    {
        // androidAudioManager.Call()
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
