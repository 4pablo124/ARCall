using System.Collections.Generic;
using UnityEngine;

public static class Colors
{
    public class Entry
    {
        public Entry(string name, string hex)
        {
            this.name = name;
            this.hex = hex;
            Color color;
            ColorUtility.TryParseHtmlString(hex, out color);
            this.color = color;
        }
        public string name;
        public string hex;
        public Color color;
    }

    public static List<Entry> colors = new List<Entry>(){
        new Entry("darkGreen","#57CC99"),
        new Entry("green","#6BDC99"),
        new Entry("lightGreen","#80ED99"),
        new Entry("black","#2E2E2E"),
        new Entry("red","#DC6B6D"),
        new Entry("blue","#6BD4DC"),
        new Entry("yellow","#FFF64A"),
    };

    public static Color GetColor(string name)
    {
        var entry = colors.Find(c => c.name == name);
        if (entry != null)
        {
            return entry.color;
        }
        return Color.white;
    }
    public static string GetHex(string name)
    {
        var entry = colors.Find(c => c.name == name);
        if (entry != null)
        {
            return entry.hex;
        }
        return "#FFFFFF";
    }
}