using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UI_TestSuite_RegisterName : TestDependenciesSetUp
{
    private Button registerBtn;
    private TMP_InputField nameInput;

    [UnitySetUp]
    public IEnumerator UnitySetUp()
    {
        SceneManager.LoadScene("RegisterName");
        yield return null;
        registerBtn = GameObject.Find("RegisterBtn").GetComponent<Button>();
        nameInput = GameObject.Find("NameInput").GetComponent<TMP_InputField>();
    }

    [UnityTearDown]
    public IEnumerator UnityTearDown()
    {
        yield return null;
    }

    [UnityTest]
    public IEnumerator Button_Register_LeadsTo_Main()
    {
        nameInput.text = "test";
        yield return new WaitUntil(() => registerBtn.interactable);
        registerBtn.onClick.Invoke();
        yield return new WaitForSecondsRealtime(1);


        Assert.AreEqual(SceneManager.GetActiveScene().name, "Main");
    }

    [UnityTest]
    public IEnumerator Button_Register_RegistersName()
    {
        var actualUsername = UserManager.CurrentUser.username;

        nameInput.text = "_test12345";
        yield return new WaitUntil(() => registerBtn.interactable);
        registerBtn.onClick.Invoke();
        yield return new WaitForSecondsRealtime(1);

        Assert.AreNotEqual(actualUsername,UserManager.CurrentUser.username);
        Assert.AreEqual("_test12345",UserManager.CurrentUser.username);

        UserManager.ChangeUsername(actualUsername);
    }
}
