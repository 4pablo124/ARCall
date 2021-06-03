using UnityEngine;
using System;

public class MyRecorder : MonoBehaviour
{
    public event Action<float[]> OnAudioReady;

    const int samplingFrequency = 48000;
    const int lengthSeconds = 1;

    public static bool muted;

    AudioClip clip = null;
    int head = 0;
    float[] processBuffer = new float[512];
    float[] microphoneBuffer = new float[lengthSeconds * samplingFrequency];
    float[] mutedBuffer = new float[lengthSeconds * samplingFrequency];
    AndroidJavaObject audioManager;


    public float GetRMS()
    {
        if(processBuffer != null){
            float sum = 0.0f;
            foreach (var sample in processBuffer)
            {
                sum += sample * sample;
            }
            return Mathf.Sqrt(sum / processBuffer.Length);
        }else{
            return 0f;
        }
            
    }

    private void Awake() {
#if UNITY_ANDROID && !UNITY_EDITOR
        try{
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            audioManager = activity.Call<AndroidJavaObject>("getSystemService", "audio");
            // Set comunication mode
            var mode2 = audioManager.Call<Int32>("getMode");
            Debug.Log("Mode was set to: " + mode2);
            audioManager.Call("setMode", 3);
            mode2 = audioManager.Call<Int32>("getMode");
            Debug.Log("Mode is now set to: " + mode2);

            // Set speakers
            bool isSpeakers = audioManager.Call<Boolean>("isSpeakerphoneOn");
            Debug.Log("Speakers were set to: " + isSpeakers);
            audioManager.Call("setSpeakerphoneOn", true);
            isSpeakers = audioManager.Call<Boolean>("isSpeakerphoneOn");
            Debug.Log("Speakers are now set to: " + isSpeakers);
 
        }catch (Exception ex){
             Debug.Log(ex.ToString());
        }
#endif
    }

    void Start()
    {
        clip = Microphone.Start(null, true, lengthSeconds, samplingFrequency);
    }

    public void toggleMute(){
        if(!muted){
            Microphone.End(null);
            Destroy(clip);
            OnAudioReady?.Invoke(null);
        }else{
            clip = Microphone.Start(null, true, lengthSeconds, samplingFrequency);
        }

        muted = !muted;
    }
    
    private void OnApplicationFocus(bool hasFocus) {
        if(!hasFocus){
            Microphone.End(null);
            Destroy(clip);
            OnAudioReady?.Invoke(null);
        }else{
            clip = Microphone.Start(null, true, lengthSeconds, samplingFrequency);  
        }
    }

    void Update()
    {
        if(!muted){

            var position = Microphone.GetPosition(null);
            if (position <= 0 || head == position)
            {
                return;
            }
            clip.GetData(microphoneBuffer, 0);

            #if UNITY_ANDROID && !UNITY_EDITOR
            Debug.Log("Speakers are now set to: " + audioManager.Call<Boolean>("isSpeakerphoneOn"));
            for (int i = 0; i < microphoneBuffer.Length; i++)
            {
                microphoneBuffer[i] = microphoneBuffer[i] * 10.0f;
            }
            #endif

            while (GetDataLength(microphoneBuffer.Length, head, position) > processBuffer.Length)
            {
                var remain = microphoneBuffer.Length - head;
                if (remain < processBuffer.Length)
                {
                    Array.Copy(microphoneBuffer, head, processBuffer, 0, remain);
                    Array.Copy(microphoneBuffer, 0, processBuffer, remain, processBuffer.Length - remain);
                }
                else
                {
                    Array.Copy(microphoneBuffer, head, processBuffer, 0, processBuffer.Length);
                }

                OnAudioReady?.Invoke(processBuffer);

                head += processBuffer.Length;
                if (head > microphoneBuffer.Length)
                {
                    head -= microphoneBuffer.Length;
                }
            }
        }
    }

    static int GetDataLength(int bufferLength, int head, int tail)
    {
        if (head < tail)
        {
            return tail - head;
        }
        else
        {
            return bufferLength - head + tail;
        }
    }
}