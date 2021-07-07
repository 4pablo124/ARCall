using UnityEngine;
using System;
using UnityEngine.Android;

public class MyRecorder : MonoBehaviour
{
    public event Action<float[]> OnAudioReady;

    const int samplingFrequency = 48000;
    const int lengthSeconds = 1;

    public static bool muted = true;

    AudioClip clip = null;
    int head = 0;
    float[] processBuffer = new float[512];
    float[] microphoneBuffer = new float[lengthSeconds * samplingFrequency];
    float[] mutedBuffer = new float[lengthSeconds * samplingFrequency];
    AndroidJavaObject audioManager;

    int defaultMode;
    bool defaultIsSpeakerphone;

    PermissionCallbacks microphoneCallbacks;

    // Mono methods
    private void Awake() {
        if(Application.platform == RuntimePlatform.Android){ 
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            audioManager = activity.Call<AndroidJavaObject>("getSystemService", "audio");

            defaultMode = audioManager.Call<Int32>("getMode");
            defaultIsSpeakerphone = audioManager.Call<Boolean>("isSpeakerphoneOn");

        }
    }

    public void SetModeAndSpeakerphone(int mode, bool isSpeakerphoneOn){
        audioManager.Call("setMode", mode);
        audioManager.Call("setSpeakerphoneOn", isSpeakerphoneOn);
    }

    void Start()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone)){
            muted = false;
            StartRecording();
        }else{
            microphoneCallbacks = new PermissionCallbacks();
            microphoneCallbacks.PermissionGranted += MicrophonePermissionGranted;
            microphoneCallbacks.PermissionDenied += MicrophonePermissionDenied;
            microphoneCallbacks.PermissionDeniedAndDontAskAgain += MicrophonePermissionDeniedAndDontAskAgain;
            Permission.RequestUserPermission(Permission.Microphone);
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
            

            #if PLATFORM_ANDROID
                for (int i = 0; i < microphoneBuffer.Length; i++)
                {
                    microphoneBuffer[i] = microphoneBuffer[i] * 50.0f;
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

    //Permissions Callbacks
    private void MicrophonePermissionGranted(string permissionName){
        muted = false;
        StartRecording();
    }
    private void MicrophonePermissionDenied(string permissionName){
        Permission.RequestUserPermission(Permission.Microphone);
    }
    private void MicrophonePermissionDeniedAndDontAskAgain(string permissionName){
        UISceneNav.loadScene("Main");
    }



    // Microphone control
    private void StartRecording(){
        muted = false;

        if(Application.platform == RuntimePlatform.Android) SetModeAndSpeakerphone(3,true);

        clip = Microphone.Start(null, true, lengthSeconds, samplingFrequency);
    }

    private void StopRecording(){
        muted = true;

        if(Application.platform == RuntimePlatform.Android) SetModeAndSpeakerphone(defaultMode,defaultIsSpeakerphone);
        
        Microphone.End(null);
        Destroy(clip);
        OnAudioReady?.Invoke(null);
    }

    public void toggleMute(){
        if(!muted){
            StopRecording();
        }else{
            StartRecording();
        }
    }
    


    private void OnApplicationPause(bool paused) {
        if(paused){
            StopRecording();
        }else{
            StartRecording(); 
        }
    }

    private void OnDestroy() {
        StopRecording();
    }


    //Aux methods
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

    // Public methods
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
}