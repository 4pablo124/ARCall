using System;
using UnityEngine;

public class ClientManager : MonoBehaviour
{

    public event Action<string> OnToolSelected;
    public event Action OnUndo;
    public event Action<string> OnDelete;
    public event Action<string> OnColorSelected;


    public void SelectTool(string toolName){
        OnToolSelected?.Invoke(toolName);
    }
    public void UndoDrawing(){
        OnUndo?.Invoke();
    }

    public void DeleteDrawings(string peer){
        OnDelete?.Invoke(peer);
    }

    public void SelectColor(string color){
        Debug.Log("clientmanager "+color);
        OnColorSelected?.Invoke(color);
    }

}
