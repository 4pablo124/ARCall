using System.Linq;
using UnityEngine;

/// <summary>
/// Controla la pulsación del botón de retroceso
/// </summary>
public class BackButtonController : MonoBehaviour
{

    private readonly string[] invalidScenes = new[]{
        "RegisterPhone",
        "RegisterName"
    };

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Marca el <see cref="GameObject"/> asociado para que no sea destruido al cambiar de escena</para>
    /// </summary>
    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// Llamada al comienzo de cada fotograma
    /// Comprueba si se ha pulsado el botón de retroceso en Android o la tecla <c>Escape</c> en Escritorio
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!invalidScenes.Contains(MySceneManager.CurrentScene().name))
            {
                MySceneManager.BackScene();
            }
        }
    }
}
