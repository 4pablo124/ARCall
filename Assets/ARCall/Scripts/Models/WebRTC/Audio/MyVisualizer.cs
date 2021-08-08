using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyVisualizer : MonoBehaviour
{

    public GameObject volume;

    private MyPlayer player;
    private MyRecorder recorder;
    private float normalizedValue;
    private float scale;
    public float max = 1.0f;
    public  float min = 0.75f;

    private void Start() {
        player = GetComponent<MyPlayer>();
        recorder = GetComponent<MyRecorder>();
    }

    void Update()
    {
        normalizedValue = player != null ? player.GetRMS() * 100.0f : recorder.GetRMS() * 100.0f;

        scale = Mathf.Clamp(normalizedValue * (max - min) + min, min, max);
        volume.transform.localScale = new Vector3(scale, scale, 1.0f);
    }
}
