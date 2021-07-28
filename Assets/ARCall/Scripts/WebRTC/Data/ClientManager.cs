using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{

    public event Action<string> OnToolSelected;
    public event Action OnUndo;
    public event Action<string> OnDelete;
    public event Action<string> OnColorChanged;

    public InputManager inputManager;

    private void Awake() {
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
    }

    public void SelectTool(string toolName){
        inputManager.currentClientTool = toolName;
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
