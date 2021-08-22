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
       isRecording=false;
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused && !TouchScreenKeyboard.visible) { StopRecording(); }
    }
}
