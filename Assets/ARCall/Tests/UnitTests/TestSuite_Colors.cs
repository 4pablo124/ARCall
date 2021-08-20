using NUnit.Framework;
using UnityEngine;

public class TestSuite_Colors
{
    // A Test behaves as an ordinary method
    [Test]
    public void GetColor_GetsColor()
    {
        Color colorTest;
        ColorUtility.TryParseHtmlString("#6BDC99", out colorTest);

        Color color = Colors.GetColor("green");

        Assert.AreEqual(colorTest, color);
    }

    [Test]
    public void GetHex_GetsHex()
    {
        string hexTest = "#6BDC99";

        string hex = Colors.GetHex("green");

        Assert.AreEqual(hexTest, hex);
    }

}
