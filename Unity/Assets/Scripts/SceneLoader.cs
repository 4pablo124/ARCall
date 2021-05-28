using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SceneLoader: MonoBehaviour
{

    public static bool firstLoad = true;

    // Start is called before the first frame update
    void Start()
    {
        if (firstLoad) {
            Application.Unload();
            firstLoad = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // boton de atras sale de unity
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Unload();
    }


    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
