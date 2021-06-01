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

    public void ToggleMute(){
        source.mute = !source.mute;
    }

    void OnEnable()
    {
        source = GetComponent<AudioSource>();
        source.clip = AudioClip.Create("Loopback", audioClipLength, (int)channels, (int)frequency, false);
        source.loop = true;
        decoder = GetComponent<MyDecoder>();
        decoder.OnDecoded += OnDecoded;
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
