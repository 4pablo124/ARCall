using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UI_TestSuite_Main : TestDependenciesSetUp
{
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("Main");
        yield return null;

    }

    [UnityTest]
    public IEnumerator Button_Create_LeadsTo_CreateRoom()
    {
        var createBtn = GameObject.Find("CreateBtn").GetComponent<Button>();
        createBtn.onClick.Invoke();
        yield return null;


        Assert.AreEqual(SceneManager.GetActiveScene().name, "CreateRoom");
    }

    [UnityTest]
    public IEnumerator Button_Join_LeadsTo_JoinRoom()
    {
        var joinBtn = GameObject.Find("JoinBtn").GetComponent<Button>();
        joinBtn.onClick.Invoke();
        yield return null;

        Assert.AreEqual(SceneManager.GetActiveScene().name, "JoinRoom");
    }

    [UnityTest]
    public IEnumerator Button_Rec_LeadsTo_Record()
    {
        var recBtn = GameObject.Find("RecBtn").GetComponent<Button>();
        recBtn.onClick.Invoke();
        yield return null;

        Assert.AreEqual(SceneManager.GetActiveScene().name, "Record");
    }

    [UnityTest]
    public IEnumerator Button_Rec_LeadsTo_RegisterPhone()
    {
        var auth = UserManager.Auth;

        var signOffBtn = GameObject.Find("SignOffBtn").GetComponent<Button>();
        signOffBtn.onClick.Invoke();
        yield return null;

        Assert.AreEqual(SceneManager.GetActiveScene().name, "RegisterPhone");

        UserManager.LogIn(auth);
    }

    [UnityTest]
    public IEnumerator Button_Rec_SignsUserOff()
    {
        var auth = UserManager.Auth;
        Assert.NotNull(UserManager.CurrentUser);

        var signOffBtn = GameObject.Find("SignOffBtn").GetComponent<Button>();
        signOffBtn.onClick.Invoke();
        yield return null;

        Assert.AreEqual(SceneManager.GetActiveScene().name, "RegisterPhone");

        Assert.Null(UserManager.CurrentUser.username);
        Assert.Null(UserManager.CurrentUser.phoneNumber);
        UserManager.LogIn(auth);
    }
}
