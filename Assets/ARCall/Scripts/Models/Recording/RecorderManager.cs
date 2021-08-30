using UnityEngine;

/// <summary>
/// Maneja la lógica de la grabación de tutoriales
/// </summary>
public class RecorderManager : MonoBehaviour
{
    private bool isRecording;
    private AndroidUtils androidUtils;

    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Inicializa los modelos y elementos de la interfaz</para>
    /// </summary>
    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        androidUtils = GetComponent<AndroidUtils>();
    }

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Se suscribe a los eventos</para>
    /// </summary>
    private void Start()
    {
        androidUtils.onStopRecord += onStopRecord;
    }

    /// <summary>
    /// Alterna entre comenzar la grabación o pararla
    /// </summary>
    /// <returns>Si esta actualmente grabando</returns>
    public bool ToggleRecord()
    {
        if (!isRecording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
        return isRecording = !isRecording;
    }

    /// <summary>
    /// Comienza la grabación
    /// </summary>
    void StartRecording()
    {
        androidUtils.StartRecording();
    }

    /// <summary>
    /// Para la grabación
    /// </summary>
    void StopRecording()
    {
        androidUtils.StopRecording();
    }

    /// <summary>
    /// Llamada cuando se para la grabación
    /// </summary>
    void onStopRecord()
    {
       isRecording=false;
    }

    /// <summary>
    /// Llamada cuando el foco de la aplicación cambia
    /// </summary>
    /// <param name="focused">Si al aplicación actualmente esta en primer plano</param>
    private void OnApplicationFocus(bool focused)
    {
        if (!focused && !TouchScreenKeyboard.visible) { StopRecording(); }
    }
}
