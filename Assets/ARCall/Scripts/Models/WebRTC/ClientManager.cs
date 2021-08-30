using System;
using UnityEngine;

/// <summary>
/// Maneja la lógica las acciones del cliente
/// </summary>
public class ClientManager : MonoBehaviour
{
    /// <summary>
    /// Evento lanzado cuando se selecciona una herramienta
    /// </summary>
    public event Action<string> OnToolSelected;
    /// <summary>
    /// Evento lanzado cuando se retrocede una acción
    /// </summary>>
    public event Action OnUndo;
    /// <summary>
    /// Evento lanzado cuando se borran los trazos o maradores
    /// </summary>
    public event Action<string> OnDelete;
    /// <summary>
    /// Evento lanzado cuando se selecciona un color
    /// </summary>
    public event Action<string> OnColorSelected;

    /// <summary>
    /// Selecciona una herramienta
    /// </summary>
    /// <param name="toolName">Nombre de la herramienta</param>
    public void SelectTool(string toolName)
    {
        OnToolSelected?.Invoke(toolName);
    }

    /// <summary>
    /// Retrocede una acción
    /// </summary>
    public void UndoDrawing()
    {
        OnUndo?.Invoke();
    }

    /// <summary>
    /// Elimina los trazos o herramientas de un par
    /// </summary>
    /// <param name="peer">Par</param>
    public void DeleteDrawings(string peer)
    {
        OnDelete?.Invoke(peer);
    }

    /// <summary>
    /// Selecciona un color
    /// </summary>
    /// <param name="color">Color</param>/
    public void SelectColor(string color)
    {
        OnColorSelected?.Invoke(color);
    }

}
