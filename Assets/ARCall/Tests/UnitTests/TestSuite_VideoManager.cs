using NUnit.Framework;
using System.Collections;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class TestSuite_VideoManager
{
    private RawImage videoRawImage;
    private VideoManager videoManager;
    private GameObject noVideoAR;
    private GameObject noVideoUI;

    [UnityOneTimeSetUp]
    public IEnumerator UnityOneTimeSetup()
    {
        SceneManager.LoadScene("Test_VideoManager");
        yield return null;
        WebRTC.Initialize(type: EncoderType.Software, limitTextureSize: true);
        videoRawImage = GameObject.Find("VideoRawImage").GetComponent<RawImage>();
        videoManager = GameObject.Find("VideoManager").GetComponent<VideoManager>();
        noVideoAR = videoManager.transform.Find("NoVideoAR")?.gameObject;
        noVideoUI = videoManager.transform.Find("NoVideoUI")?.gameObject;
    }

    [UnityOneTimeTearDown]
    public IEnumerator UnityOneTimeTearDown()
    {
        WebRTC.Dispose();
        yield return null;
    }

    [UnityTest]
    public IEnumerator RecordCamera_RecordsCamera()
    {
        Assert.Null(videoRawImage.texture);

        videoManager.RecordCamera();
        yield return null;

        Assert.NotNull(videoRawImage.texture);
    }

    [UnityTest]
    public IEnumerator ShowVideo_DisablesCamera()
    {
        Assert.False(noVideoAR.activeSelf);
        Assert.False(noVideoUI.activeSelf);
        Assert.True(ARToolManager.hostDrawings.activeSelf);
        Assert.True(ARToolManager.hostGuides.activeSelf);
        Assert.True(ARToolManager.clientDrawings.activeSelf);
        Assert.True(ARToolManager.clientGuides.activeSelf);

        videoManager.ShowVideo(false);
        yield return null;

        Assert.True(noVideoAR.activeSelf);
        Assert.True(noVideoUI.activeSelf);
        Assert.False(ARToolManager.hostDrawings.activeSelf);
        Assert.False(ARToolManager.hostGuides.activeSelf);
        Assert.False(ARToolManager.clientDrawings.activeSelf);
        Assert.False(ARToolManager.clientGuides.activeSelf);

        videoManager.ShowVideo(true);
        yield return null;
    }
}
