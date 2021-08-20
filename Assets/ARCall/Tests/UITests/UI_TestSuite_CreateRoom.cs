using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UI_TestSuite_CreateRoom : TestDependenciesSetUp
{
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("CreateRoom");
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        yield return null;
    }

    [UnityTest]
    public IEnumerator Button_Join_LeadsTo_Host()
    {
        var joinBtn = GameObject.Find("JoinBtn").GetComponent<Button>();
        yield return new WaitUntil(() => joinBtn.interactable);
        joinBtn.onClick.Invoke();
        yield return null;
        Assert.AreEqual("Host", SceneManager.GetActiveScene().name);
        yield return new WaitForSecondsRealtime(1);
    }

    [Test]
    public void Button_Share_OpensShareMenu()
    {
        Assert.Ignore("[MANUAL TEST]");
    }

    [UnityTest]
    public IEnumerator ShowsContactsIfPhoneRegistered()
    {
        var actualUser = UserManager.CurrentUser;
        UserManager.CurrentUser = new User("Test", "123456789");
        SceneManager.LoadScene("CreateRoom");
        yield return null;

        Assert.NotNull(GameObject.Find("ScrollContactos"));

        UserManager.CurrentUser = actualUser;
    }

    [UnityTest]
    public IEnumerator DoenstShowContactsIfPhoneNotRegistered()
    {
        var actualUser = UserManager.CurrentUser;
        UserManager.CurrentUser = new User("Test");
        SceneManager.LoadScene("CreateRoom");
        yield return null;

        Assert.Null(GameObject.Find("ScrollContactos"));

        UserManager.CurrentUser = actualUser;
    }
}
