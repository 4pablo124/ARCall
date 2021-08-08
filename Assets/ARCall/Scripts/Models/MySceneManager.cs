using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MySceneManager {
    public static Stack<int> SceneStack = new Stack<int>();

    public static void LoadScene(string scene){
        SceneStack.Push(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene(scene);
    }

    public static Scene CurrentScene(){
        return SceneManager.GetActiveScene();
    }
    public static void BackScene(){
        if(SceneManager.GetActiveScene().buildIndex > 1){
            SceneManager.LoadScene(SceneStack.Pop());
        }else{
            Application.Quit();   
        }
    }
}
