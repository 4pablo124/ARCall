using System;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;

public class VideoManager {
    public static MediaStream RecordLiveVideo(ref Camera camera, ref RawImage videoImage){
        MediaStream video;
        // Capturamos Video
        Debug.Log($"Capturando video con aspect ratio de: {camera.aspect}");
        var height = (int)Math.Round(PeerConnection.width/camera.aspect);
        video = camera.CaptureStream(PeerConnection.width, height, (int)PeerConnection.bitrate);


        videoImage.GetComponent<AspectRatioFitter>().aspectRatio = camera.aspect;
        videoImage.texture = camera.targetTexture;
        videoImage.color = Color.white;

        return video;
    }
}