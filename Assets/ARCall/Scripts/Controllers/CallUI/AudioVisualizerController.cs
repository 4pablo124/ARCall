using UnityEngine;

/// <summary>
/// Controla el visualizador de audio
/// </summary>
public class AudioVisualizerController : MonoBehaviour
{
    /// <summary>
    /// Visualizador de audio
    /// </summary>
    public GameObject volumeVisualizer;
    private float normalizedValue;
    private float scale;
    public float max = 1.0f;
    public float min = 0.6f;
    private AudioManager audioManager;
    private AudioSource audioSource;
    private float volume;


    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Inicializa los objetos necesarios</para>
    /// </summary>
    private void Start()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    /// <summary>
    /// Llamada al comienzo de cada fotograma
    /// <para>Modifica la escala del visualizador en función del volumen</para>
    /// </summary>
    private void Update()
    {
        volume = audioManager.GetVolume(audioSource);
        scale = Mathf.Clamp(volume * (max - min) + min, min, max);
        volumeVisualizer.transform.localScale = new Vector3(scale, scale, 1.0f);
    }
}
