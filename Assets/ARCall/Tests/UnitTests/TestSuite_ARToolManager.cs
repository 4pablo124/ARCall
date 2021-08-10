using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestSuite_ARToolManager
{

    ARToolManager aRToolManager;

    [UnityOneTimeSetUp]
    public IEnumerator OnetimeSetup(){
        SceneManager.LoadScene("Test_ARTools");
        yield return null;
        aRToolManager = GameObject.Find("ARToolManager").GetComponent<ARToolManager>();
    }

    private GameObject getTool(PeerType peer, string toolName){
        var tool = GameObject.Find(peer.ToString()+"Tools").transform.Find(toolName).gameObject;
        tool.SetActive(false);
        return tool;
    }


    // SelectTool()

    [Test]
    public void SelectTool_HostSelectsARBrush(){
        var tool = getTool(PeerType.Host, "ARBrush");
        Assert.False(tool.activeSelf);

        aRToolManager.SelectTool(PeerType.Host,"ARBrush");

        Assert.True(tool.activeSelf);
        tool.SetActive(false);
    }

    [Test]
    public void SelectTool_ClientSelectsARBrush(){
        var tool = getTool(PeerType.Client, "ARBrush");
        Assert.False(tool.activeSelf);

        aRToolManager.SelectTool(PeerType.Client,"ARBrush");

        Assert.True(tool.activeSelf);
        tool.SetActive(false);
    }

    [Test]
    public void SelectTool_HostSelectsARPointer(){
        var tool = getTool(PeerType.Host, "ARPointer");
        Assert.False(tool.activeSelf);

        aRToolManager.SelectTool(PeerType.Host,"ARPointer");

        Assert.True(tool.activeSelf);
        tool.SetActive(false);
    }

    [Test]
    public void SelectTool_ClientSelectsARPointer(){
        var tool = getTool(PeerType.Client, "ARPointer");
        Assert.False(tool.activeSelf);

        aRToolManager.SelectTool(PeerType.Client,"ARPointer");

        Assert.True(tool.activeSelf);
        tool.SetActive(false);
    }

    [Test]
    public void SelectTool_HostSelectsARMarker(){
        var tool = getTool(PeerType.Host, "ARMarker");
        Assert.False(tool.activeSelf);

        aRToolManager.SelectTool(PeerType.Host,"ARMarker");

        Assert.True(tool.activeSelf);
        tool.SetActive(false);
    }

    [Test]
    public void SelectTool_ClientSelectsARMarker(){
        var tool = getTool(PeerType.Client, "ARMarker");
        Assert.False(tool.activeSelf);

        aRToolManager.SelectTool(PeerType.Client,"ARMarker");

        Assert.True(tool.activeSelf);
        tool.SetActive(false);
    }

    [Test]
    public void SelectTool_HostSelectsARText(){
        var tool = getTool(PeerType.Host, "ARText");
        Assert.False(tool.activeSelf);

        aRToolManager.SelectTool(PeerType.Host,"ARText");

        Assert.True(tool.activeSelf);
        tool.SetActive(false);
    }

    [Test]
    public void SelectTool_ClientSelectsARText(){
        var tool = getTool(PeerType.Client, "ARText");
        Assert.False(tool.activeSelf);

        aRToolManager.SelectTool(PeerType.Client,"ARText");

        Assert.True(tool.activeSelf);
        tool.SetActive(false);
    }

    
    // SelectColor

    [Test]
    public void SelectColor_HostSelectsRed(){
        aRToolManager.hostMaterial = null;

        aRToolManager.SelectColor(PeerType.Host,"DC6B6D");

        Assert.True(aRToolManager.hostMaterial == aRToolManager.ColorRed);

        aRToolManager.hostMaterial = null;
    }

    [Test]
    public void SelectColor_ClientSelectsRed(){
        aRToolManager.clientMaterial = null;

        aRToolManager.SelectColor(PeerType.Client,"DC6B6D");

        Assert.True(aRToolManager.clientMaterial == aRToolManager.ColorRed);

        aRToolManager.clientMaterial = null;
    }

    [Test]
    public void SelectColor_HostSelectsGreen(){
        aRToolManager.hostMaterial = null;

        aRToolManager.SelectColor(PeerType.Host,"6BDC99");

        Assert.True(aRToolManager.hostMaterial == aRToolManager.ColorGreen);

        aRToolManager.hostMaterial = null;
    }

    [Test]
    public void SelectColor_ClientSelectsGreen(){
        aRToolManager.clientMaterial = null;

        aRToolManager.SelectColor(PeerType.Client,"6BDC99");

        Assert.True(aRToolManager.clientMaterial == aRToolManager.ColorGreen);

        aRToolManager.clientMaterial = null;
    }

    [Test]
    public void SelectColor_HostSelectsBlue(){
        aRToolManager.hostMaterial = null;

        aRToolManager.SelectColor(PeerType.Host,"6BD4DC");

        Assert.True(aRToolManager.hostMaterial == aRToolManager.ColorBlue);

        aRToolManager.hostMaterial = null;
    }

    [Test]
    public void SelectColor_ClientSelectsBlue(){
        aRToolManager.clientMaterial = null;

        aRToolManager.SelectColor(PeerType.Client,"6BD4DC");

        Assert.True(aRToolManager.clientMaterial == aRToolManager.ColorBlue);

        aRToolManager.clientMaterial = null;
    }

    [Test]
    public void SelectColor_HostSelectsYellow(){
        aRToolManager.hostMaterial = null;

        aRToolManager.SelectColor(PeerType.Host,"FFF64A");

        Assert.True(aRToolManager.hostMaterial == aRToolManager.ColorYellow);

        aRToolManager.hostMaterial = null;
    }

    [Test]
    public void SelectColor_ClientSelectsYellow(){
        aRToolManager.clientMaterial = null;

        aRToolManager.SelectColor(PeerType.Client,"FFF64A");

        Assert.True(aRToolManager.clientMaterial == aRToolManager.ColorYellow);

        aRToolManager.clientMaterial = null;
    }


    // UndoDrawing()

    [UnityTest]
    public IEnumerator UndoDrawing_HostUndoesDrawing(){
        var drawings = GameObject.Find("HostDrawings").transform;
        var guides = GameObject.Find("HostGuides").transform;
        Assert.True(drawings.childCount == 2);
        Assert.True(guides.childCount == 2);

        aRToolManager.UndoDrawing("Host");
        yield return null;

        Assert.True(drawings.childCount == 1);
        Assert.True(guides.childCount == 1);

        new GameObject().transform.SetParent(drawings);
        new GameObject().transform.SetParent(guides);
    }

    [UnityTest]
    public IEnumerator UndoDrawing_ClientUndoesDrawing(){
        var drawings = GameObject.Find("ClientDrawings").transform;
        var guides = GameObject.Find("ClientGuides").transform;
        Assert.True(drawings.childCount == 2);
        Assert.True(guides.childCount == 2);

        aRToolManager.UndoDrawing("Client");
        yield return null;

        Assert.True(drawings.childCount == 1);
        Assert.True(guides.childCount == 1);
        new GameObject().transform.SetParent(drawings);
        new GameObject().transform.SetParent(guides);
    }


    // DeleteDrawings()

    [UnityTest]
    public IEnumerator DeleteDrawings_HostDeletesDrawings(){
        var drawings = GameObject.Find("HostDrawings").transform;
        var guides = GameObject.Find("HostGuides").transform;
        Assert.True(drawings.childCount == 2);
        Assert.True(guides.childCount == 2);

        aRToolManager.DeleteDrawings("Host");
        yield return null;

        Assert.True(drawings.childCount == 0);
        Assert.True(guides.childCount == 0);

        new GameObject().transform.SetParent(drawings);
        new GameObject().transform.SetParent(drawings);
        new GameObject().transform.SetParent(guides);
        new GameObject().transform.SetParent(guides);
    }

    [UnityTest]
    public IEnumerator DeleteDrawings_ClientDeletesDrawings(){
        var drawings = GameObject.Find("ClientDrawings").transform;
        var guides = GameObject.Find("ClientGuides").transform;
        Assert.True(drawings.childCount == 2);
        Assert.True(guides.childCount == 2);

        aRToolManager.DeleteDrawings("Client");
        yield return null;

        Assert.True(drawings.childCount == 0);
        Assert.True(guides.childCount == 0);

        new GameObject().transform.SetParent(drawings);
        new GameObject().transform.SetParent(drawings);
        new GameObject().transform.SetParent(guides);
        new GameObject().transform.SetParent(guides);
    }

    [UnityTest]
    public IEnumerator DeleteDrawings_BothDeletesDrawings(){
        var hostDrawings = GameObject.Find("ClientDrawings").transform;
        var hostGuides = GameObject.Find("ClientGuides").transform;
        var clientDrawings = GameObject.Find("HostDrawings").transform;
        var clientGuides = GameObject.Find("HostGuides").transform;
        Assert.True(hostDrawings.childCount == 2);
        Assert.True(hostGuides.childCount == 2);
        Assert.True(clientDrawings.childCount == 2);
        Assert.True(clientGuides.childCount == 2);

        aRToolManager.DeleteDrawings("Both");
        yield return null;

        Assert.True(hostDrawings.childCount == 0);
        Assert.True(hostGuides.childCount == 0);
        Assert.True(clientDrawings.childCount == 0);
        Assert.True(clientGuides.childCount == 0);

        new GameObject().transform.SetParent(hostDrawings);
        new GameObject().transform.SetParent(hostDrawings);
        new GameObject().transform.SetParent(hostGuides);
        new GameObject().transform.SetParent(hostGuides);
        new GameObject().transform.SetParent(clientDrawings);
        new GameObject().transform.SetParent(clientDrawings);
        new GameObject().transform.SetParent(clientGuides);
        new GameObject().transform.SetParent(clientGuides);
    }


    // PlaceGuide()

    [UnityTest]
    public IEnumerator PlaceGuide_HostPlacesGuide(){
        var guides = GameObject.Find("HostGuides").transform;
        Assert.True(guides.childCount == 2);

        var guide = aRToolManager.PlaceGuide(PeerType.Host,new GameObject().transform);
        yield return null;

        Assert.True(guides.childCount == 3);

        GameObject.Destroy(guide);
    }

    [UnityTest]
    public IEnumerator PlaceGuide_ClientPlacesGuide(){
        var guides = GameObject.Find("ClientGuides").transform;
        Assert.True(guides.childCount == 2);

        var guide = aRToolManager.PlaceGuide(PeerType.Client,new GameObject().transform);
        yield return null;

        Assert.True(guides.childCount == 3);

        GameObject.Destroy(guide);
    }

    [UnityTest]
    public IEnumerator PlaceGuide_HostGuideHasSelectedColor(){
        var guides = GameObject.Find("HostGuides").transform;
        aRToolManager.hostMaterial = aRToolManager.ColorRed;
        
        var guide = aRToolManager.PlaceGuide(PeerType.Host,new GameObject().transform);
        yield return null;

        Assert.True(guide.GetComponent<SpriteRenderer>().color == aRToolManager.hostMaterial.color);

        GameObject.Destroy(guide);
    }

    [UnityTest]
    public IEnumerator PlaceGuide_ClientGuideHasSelectedColor(){
        var guides = GameObject.Find("ClientGuides").transform;
        aRToolManager.clientMaterial = aRToolManager.ColorRed;
        
        var guide = aRToolManager.PlaceGuide(PeerType.Client,new GameObject().transform);
        yield return null;

        Assert.True(guide.GetComponent<SpriteRenderer>().color == aRToolManager.clientMaterial.color);

        GameObject.Destroy(guide);
    }
}
