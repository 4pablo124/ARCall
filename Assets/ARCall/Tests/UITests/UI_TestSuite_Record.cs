using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UI_TestSuite_Record : TestDependenciesSetUp
{
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("Record");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Button_SelectTool_ShowsToolBar()
    {
        var selectedTool = GameObject.Find("SelectedToolBtn").GetComponent<Button>();

        Assert.Null(GameObject.Find("ARToolsBar"));

        selectedTool.onClick.Invoke();
        yield return null;

        Assert.NotNull(GameObject.Find("ARToolsBar"));
    }

    [UnityTest]
    public IEnumerator Button_SelectTool_ChangesSelectedTool()
    {
        var selectedTool = GameObject.Find("SelectedToolBtn");
        var arToolsBar = GameObject.Find("Toolbar").transform.Find("ARToolsBar");
        var tool = arToolsBar.transform.Find("Buttons").Find("Tool1").gameObject;

        var BeforeToolSprite = tool.GetComponent<Image>().sprite.name;
        var BeforeSelectedToolSprite = selectedTool.GetComponent<Image>().sprite.name;


        tool.GetComponent<Button>().onClick.Invoke();
        yield return null;

        var AfterToolSprite = tool.GetComponent<Image>().sprite.name;
        var AfterSelectedToolSprite = selectedTool.GetComponent<Image>().sprite.name;

        Assert.AreEqual(BeforeSelectedToolSprite, AfterToolSprite);
        Assert.AreEqual(BeforeToolSprite, AfterSelectedToolSprite);
    }

    [UnityTest]
    public IEnumerator Button_SelectColor_ChangesSelectedColor()
    {
        var selectedColor = GameObject.Find("SelectedColorBtn");
        var arToolsBar = GameObject.Find("Toolbar").transform.Find("ColorsBar");
        var color = arToolsBar.transform.Find("Buttons").Find("Color1").gameObject;

        var BeforeColorSprite = color.GetComponent<Image>().sprite.name;
        var BeforeSelectedColorSprite = selectedColor.GetComponent<Image>().sprite.name;


        color.GetComponent<Button>().onClick.Invoke();
        yield return null;

        var AfterColorSprite = color.GetComponent<Image>().sprite.name;
        var AfterSelectedColorSprite = selectedColor.GetComponent<Image>().sprite.name;

        Assert.AreEqual(BeforeSelectedColorSprite, AfterColorSprite);
        Assert.AreEqual(BeforeColorSprite, AfterSelectedColorSprite);
    }

}
