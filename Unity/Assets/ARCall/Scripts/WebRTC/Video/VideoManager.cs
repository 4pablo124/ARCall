using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;

public class VideoManager {
    public static MediaStream RecordLiveVideo(ref Camera camera, ref RawImage videoImage){
        MediaStream video;
        // Capturamos Video
        video = camera.CaptureStream(720, 1280, 1000000);
        Debug.Log($"Capturando stream: {video}");

        videoImage.texture = camera.targetTexture;
        videoImage.color = Color.white;
        videoImage.GetComponent<AspectRatioFitter>().aspectRatio = camera.aspect;

        return video;
    }
}