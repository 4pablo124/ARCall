using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UI_TestSuite_JoinRoom : TestDependenciesSetUp
{

    [AsyncOneTimeSetUp]
    public async Task AsyncOneTimeSetUp(){
        await DatabaseManager.ReadyUser("0007",PeerType.Host);
        await Task.Delay(1000);
    }

    [AsyncOneTimeTearDown]
    public async Task AsyncOneTimeTearDown(){
        await DatabaseManager.UnReadyUser("0007",PeerType.Host);
        await Task.Delay(1000);
    }


    [UnitySetUp]
    public IEnumerator UnitySetUp()
    {   
        SceneManager.LoadScene("JoinRoom");
        yield return null;
    }

    [AsyncTearDown]
    public async Task AsyncTearDown()
    {   
        await Task.Delay(100);
        await DatabaseManager.UnReadyUser("0007",PeerType.Client);
    }

    [UnityTest]
    public IEnumerator Button_Join_LeadsTo_Client_WithValidCode(){
        var roomIDInput = GameObject.Find("RoomIDInput")?.GetComponent<TMP_InputField>();
        var joinBtn = GameObject.Find("JoinBtn").GetComponent<Button>();
        roomIDInput.text = "0007";
        yield return new WaitUntil(() => joinBtn.interactable);
        joinBtn.onClick.Invoke();
        yield return new WaitForSecondsRealtime(1);
        Assert.AreEqual("Client", SceneManager.GetActiveScene().name);

    }

    [UnityTest]
    public IEnumerator Button_Join_ShowsError_WithInvalidCode(){
        var roomIDInput = GameObject.Find("RoomIDInput")?.GetComponent<TMP_InputField>();
        var joinBtn = GameObject.Find("JoinBtn").GetComponent<Button>();
        roomIDInput.text = "0001";
        yield return new WaitUntil(() => joinBtn.interactable);
        joinBtn.onClick.Invoke();
        yield return new WaitForSecondsRealtime(1);
        Assert.AreEqual("JoinRoom", SceneManager.GetActiveScene().name);
    }

}
