using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UISceneNav : MonoBehaviour {
    private void Awake() {
        // ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.Visible;
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            backScene();
        }
    }

    public static void loadScene(string scene){
        PersistentData.SceneStack.Push(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene(scene);
    }

    public static void backScene(){
        if(SceneManager.GetActiveScene().buildIndex != 0){
            SceneManager.LoadScene(PersistentData.SceneStack.Pop());
        }else{
            Application.Quit();   
        }
    }
}
