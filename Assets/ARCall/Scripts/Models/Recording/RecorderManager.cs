using UnityEngine;
public class RecorderManager : MonoBehaviour
{
    private bool isRecording;
    private AndroidUtils androidUtils;



    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        androidUtils = GetComponent<AndroidUtils>();
    }

    private void Start()
    {
        androidUtils.onStopRecord += onStopRecord;
    }
    public bool ToggleRecord()
    {
        if (!isRecording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
        return isRecording = !isRecording;
    }
    void StartRecording()
    {
        androidUtils.StartRecording();
    }

    void StopRecording()
    {
        androidUtils.StopRecording();
    }

    void onStopRecord()
    {

        // AndroidUtils.ShareAndroid(null,null,null,androidUtils.filePath,"video/mp4",false,null);

        // string destPath = Path.Combine(Application.temporaryCachePath,androidUtils.fileName);
        // File.Copy(androidUtils.filePath,destPath);
        // Debug.Log(destPath);
        // new NativeShare().AddFile(destPath).Share();
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused && !TouchScreenKeyboard.visible) { StopRecording(); }
    }
}
