using System.Collections.Concurrent;
using UnityEngine;
using System;


[RequireComponent(typeof(MyDecoder), typeof(AudioSource))]
public class MyPlayer : MonoBehaviour
{
    const UnityOpus.NumChannels channels = UnityOpus.NumChannels.Mono;
    const UnityOpus.SamplingFrequency frequency = UnityOpus.SamplingFrequency.Frequency_48000;
    const int audioClipLength = 1024 * 6;
    AudioSource source;
    MyDecoder decoder;
    int head = 0;
    float[] audioClipData;
    private AndroidJavaObject context;
    private AndroidJavaObject audioManager;
    private int voiceMinVol;
    private int voiceMaxVol;
    private int mediaMinVol;
    private int mediaMaxVol;
    private int originalMediaVol;

    public bool ToggleMute(){
        return source.mute = !source.mute;
    }

    void OnEnable()
    {
        source = GetComponent<AudioSource>();
        source.clip = AudioClip.Create("Loopback", audioClipLength, (int)channels, (int)frequency, false);
        source.loop = true;

        //TODO: cambiar por audiomanager
        decoder = GetComponent<MyDecoder>();
        decoder.OnDecoded += OnDecoded;
    }

    private void Start() {
        if(Application.platform == RuntimePlatform.Android){ 
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            audioManager = context.Call<AndroidJavaObject>("getSystemService", "audio");

            voiceMinVol = 0;
            mediaMinVol = 1;
            voiceMaxVol = audioManager.Call<int>("getStreamMaxVolume", 0);
            mediaMaxVol = audioManager.Call<int>("getStreamMaxVolume", 3);
            originalMediaVol = audioManager.Call<int>("getStreamVolume", 3);

        }
    }

    private float scaleValueToRange(float value, float valueMin, float valueMax, float rangeMin, float rangeMax){
        return ( (value - valueMin) / (valueMax - valueMin)) * (rangeMax - rangeMin) + rangeMin;
    }

    private void Update() {
        // Debug.Log("STREAM_MUSIC: "+audioManager.Call<int>("getStreamVolume",0));
        // Debug.Log("STREAM_VOICE_CALL: "+audioManager.Call<int>("getStreamVolume",3));

        // Debug.Log(audioManager.Call<int>("getStreamVolume", 0));
        // Debug.Log(audioManager.Call<int>("getStreamVolume", 3));

        if(Application.platform == RuntimePlatform.Android){ 
            audioManager.Call("setStreamVolume",
                3, // STREAM_MUSIC
                (int)Math.Round(scaleValueToRange(audioManager.Call<int>("getStreamVolume", 0), voiceMinVol, voiceMaxVol, mediaMinVol, mediaMaxVol)), // Usar el volumen de STREAM_VOICE_CALL
                0 // no flags
            );
        }
    }

    public float GetRMS()
    {
        if(audioClipData != null){
            float sum = 0.0f;
            foreach (var sample in audioClipData)
            {
                sum += sample * sample;
            }
            return Mathf.Sqrt(sum / audioClipData.Length);
        }else{
            return 0f;
        }
    }

    void OnDisable()
    {
        if(Application.platform == RuntimePlatform.Android){ 
            audioManager.Call("setStreamVolume",
                3, // STREAM_MUSIC
                originalMediaVol, // Usar el volumen antes de entrar en la videollamada
                0 // no flags
            );
        }
        
        decoder.OnDecoded -= OnDecoded;
        source.Stop();
    }

    void OnDecoded(float[] pcm, int pcmLength)
    {
        if(pcm != null){
            if (audioClipData == null || audioClipData.Length != pcmLength)
            {
                // assume that pcmLength will not change.
                audioClipData = new float[pcmLength];
            }
            Array.Copy(pcm, audioClipData, pcmLength);
            source.clip.SetData(audioClipData, head);
            head += pcmLength;
            if (!source.isPlaying && head > audioClipLength / 2)
            {
                source.Play();
            }
            head %= audioClipLength;
        }else{
            source.Stop();
        }
    }
}
