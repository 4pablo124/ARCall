using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class VideoTestSuite
{
    // A Test behaves as an ordinary method
    [UnityTest]
    public IEnumerator RecordLiveVideoCapturaCamara(){
        PersistentData.SetRoomID("0000");
        SceneManager.LoadScene("Host");
        yield return new WaitForSeconds(0.5f);

        Camera cam = Camera.main;
        GameObject rawObj = new GameObject();
        rawObj.AddComponent<RawImage>();
        rawObj.AddComponent<AspectRatioFitter>();
        RawImage videoImage = rawObj.GetComponent<RawImage>();

        MediaStream videoStream;

        videoStream = VideoManager.RecordLiveVideo(ref cam, ref videoImage);
        Assert.NotNull(videoStream);
        PersistentData.SetRoomID("");
    }
}
