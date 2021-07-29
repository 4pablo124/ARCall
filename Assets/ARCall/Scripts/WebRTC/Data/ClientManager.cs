using System;
using UnityEngine;

public class ClientManager : MonoBehaviour
{

    public event Action<string> OnToolSelected;
    public event Action OnUndo;
    public event Action<string> OnDelete;
    public event Action<string> OnColorChanged;

    public InputManager inputManager;


    public void SelectTool(string toolName){
        OnToolSelected?.Invoke(toolName);
    }
    public void UndoDrawing(){
        OnUndo?.Invoke();
    }

    public void DeleteDrawings(string peer){
        OnDelete?.Invoke(peer);
    }

    public void ChangeColor(string color){
        OnColorChanged?.Invoke(color);
    }

}
