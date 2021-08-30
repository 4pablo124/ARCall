
using System.Collections;
using UnityEngine;

/// <summary>
/// Redimendiona la ventana de la aplicación de escritorio para que mantenga la relación de aspecto
/// </summary>
public class WindowRes : MonoBehaviour
{
    int lastWidth;
    int lastHeight;

    bool isReseting = false;

    float aspect;
    float currentAspect;

    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Inicializa los parametros iniciales</para>
    /// </summary>
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        aspect = (float)Screen.width / Screen.height;
    }

    /// <summary>
    /// Llamada al final de cada fotograma
    /// <para>Comprueba si cambia la relación de aspecto
    /// </summary>
    void LateUpdate()
    {
        currentAspect = (float)Screen.width / Screen.height;

        if (currentAspect != aspect && !isReseting)
        {
            StartCoroutine(ChangeResolution());
        }
    }

    /// <summary>
    /// Cambia la resolución de la ventana
    /// </summary>
    /// <returns>Corutina de Unity</returns>
    IEnumerator ChangeResolution()
    {
        isReseting = true;
        if (Screen.width != lastWidth)
        {
            // user is resizing width
            var heightAccordingToWidth = Screen.width / aspect;
            Screen.SetResolution(Screen.width, (int)Mathf.Round((float)heightAccordingToWidth), false);
            lastWidth = Screen.width;
        }
        else if (Screen.height != lastHeight)
        {
            // user is resizing height
            var widthAccordingToHeigth = Screen.height * aspect;
            Screen.SetResolution((int)Mathf.Round((float)widthAccordingToHeigth), Screen.height, false);
            lastHeight = Screen.height;
        }

        yield return new WaitForSeconds(0.5f);
        isReseting = false;
    }
}