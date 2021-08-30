using UnityEngine;

public class LookAtCamera : MonoBehaviour
{

    private Camera cam;

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Iniciliazliza la cámara</para>
    /// </summary>
    private void Start()
    {
        cam = GameObject.Find("ARCamera").GetComponent<Camera>();
    }

    /// <summary>
    /// Llamada al comienzo de cada fotograma
    /// <para>Posiciona el <see cref="GameObject"/> asociado para ue apunte hacia la cámara</para>
    /// </summary>
    private void Update()
    {
        transform.LookAt(cam.transform);
    }
}
