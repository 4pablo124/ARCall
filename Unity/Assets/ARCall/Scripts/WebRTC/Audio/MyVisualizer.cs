using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyVisualizer : MonoBehaviour
{

    public Slider slider;

    private MyPlayer player;
    private MyRecorder recorder;

    private void Start() {
        player = GetComponent<MyPlayer>();
        recorder = GetComponent<MyRecorder>();
    }

    void Update()
    {
        if(player != null){
            slider.value = player.GetRMS();
        }else{
            slider.value = recorder.GetRMS();

        }
    }
}
