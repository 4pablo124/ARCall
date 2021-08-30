using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la interfaz principal de la aplicación
/// </summary>
public class MainUIController : MonoBehaviour
{

    private Button createBtn;
    private Button joinBtn;
    private Button recBtn;
    private Button signOffBtn;

    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Inicializa los modelos y elementos de la interfaz</para>
    /// </summary>
    private void Awake()
    {
        createBtn = GameObject.Find("CreateBtn").GetComponent<Button>();
        joinBtn = GameObject.Find("JoinBtn").GetComponent<Button>();
        recBtn = GameObject.Find("RecBtn").GetComponent<Button>();
        signOffBtn = GameObject.Find("SignOffBtn").GetComponent<Button>();
    }

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Asigna acciones a los botones de la interfaz</para>
    /// </summary>
    private void Start()
    {
        if(!UserManager.IsUserRegistered()){
            MySceneManager.LoadScene("RegisterPhone");
        }

        createBtn.onClick.AddListener(() =>
        {
            MySceneManager.LoadScene("CreateRoom");
        });

        joinBtn.onClick.AddListener(() =>
        {
            MySceneManager.LoadScene("JoinRoom");
        });

        recBtn.onClick.AddListener(() =>
        {
            MySceneManager.LoadScene("Record");
        });

        signOffBtn.onClick.AddListener(() =>
        {
            UserManager.LogOut();
            MySceneManager.LoadScene("RegisterPhone");
        });

    }

}
