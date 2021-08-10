using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UI_TestSuite_RegisterPhone
{
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("RegisterPhone");
        yield return null;

    }

    [Test]
    public void Button_Send_SendsCodeViaSMS()
    {
        Assert.Ignore("[MANUAL TEST]");
    }

    [Test]
    public void Button_Verify_VerifiesCodeReceivedViaSMS()
    {
        Assert.Ignore("[MANUAL TEST]");
    }

    [UnityTest]
    public IEnumerator Button_Skip_LeadsTo_RegisterName()
    {
        var skipBtn = GameObject.Find("SkipBtn").GetComponent<Button>();

        skipBtn.onClick.Invoke();
        yield return null;


        Assert.AreEqual(SceneManager.GetActiveScene().name, "RegisterName");
    }
}
