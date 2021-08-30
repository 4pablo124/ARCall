using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja la lógica del cambio de escenas
/// </summary>
public static class MySceneManager
{
    /// <summary>
    /// Pila de escenas accedidas
    /// </summary>
    public static Stack<int> SceneStack = new Stack<int>();

    /// <summary>
    /// Carga la escena seleccionada
    /// </summary>
    /// <param name="scene">Nombre de la escena</param>
    public static void LoadScene(string scene)
    {
        SceneStack.Push(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene(scene);
    }

    /// <summary>
    /// Devuelve la escena actual
    /// </summary>
    /// <returns>Escena actual</returns>
    public static Scene CurrentScene()
    {
        return SceneManager.GetActiveScene();
    }

    /// <summary>
    /// Navega a la escena anterior
    /// </summary>
    public static void BackScene()
    {
        if (SceneManager.GetActiveScene().buildIndex > 1)
        {
            SceneManager.LoadScene(SceneStack.Pop());
        }
        else
        {
            Application.Quit();
        }
    }
}
