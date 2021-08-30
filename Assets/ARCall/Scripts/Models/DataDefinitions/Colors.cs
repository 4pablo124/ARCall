using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Almacena los colores usados en el sistema
/// </summary>
public static class Colors
{
    /// <summary>
    /// Representa un color almacenado
    /// </summary>
    public class Entry
    {
        /// <summary>
        /// Constructor de un color almacenado
        /// </summary>
        /// <param name="name">Nombre del color</param>
        /// <param name="hex">Código hexadecimal del color</param>
        public Entry(string name, string hex)
        {
            this.name = name;
            this.hex = hex;
            Color color;
            ColorUtility.TryParseHtmlString(hex, out color);
            this.color = color;
        }
        /// <summary>
        /// Nombre del color
        /// </summary>
        public string name;
        /// <summary>
        /// Código hexadecimal del color
        /// </summary>
        public string hex;
        /// <summary>
        /// Representación en Unity del color
        /// </summary>
        public Color color;
    }

    /// <summary>
    /// Listado de colores almacenados
    /// </summary>
    public static List<Entry> colors = new List<Entry>(){
        new Entry("darkGreen","#57CC99"),
        new Entry("green","#6BDC99"),
        new Entry("lightGreen","#80ED99"),
        new Entry("black","#2E2E2E"),
        new Entry("red","#DC6B6D"),
        new Entry("blue","#6BD4DC"),
        new Entry("yellow","#FFF64A"),
    };

    /// <summary>
    /// Devuelve el color dado el nombre
    /// </summary>
    /// <param name="name">Nombre del color</param>
    /// <returns>Representación en Unity del color</returns>
    public static Color GetColor(string name)
    {
        var entry = colors.Find(c => c.name == name);
        if (entry != null)
        {
            return entry.color;
        }
        return Color.white;
    }

    /// <summary>
    /// Devuelve el código hezadecimal dado el nombre
    /// </summary>
    /// <param name="name">Nombre del color</param>
    /// <returns>Código hexadecimal del color</returns>
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