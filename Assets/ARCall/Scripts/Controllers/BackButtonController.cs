using System.Linq;
using UnityEngine;

public class BackButtonController : MonoBehaviour {

    private string[] invalidScenes = new []{
        "RegistroTlf",
        "Registro"
    };

    private void Start() {
        DontDestroyOnLoad(this.gameObject);    
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if(!invalidScenes.Contains(MySceneManager.CurrentScene().name)){
                Debug.Log("Back key pressed");
                MySceneManager.BackScene();
            }
        }
    }
}
