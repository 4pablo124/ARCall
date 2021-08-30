using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la interfaz de la escena <c>RegisterName</c>
/// </summary>
public class RegisterNameUIController : MonoBehaviour
{

    private TMP_InputField nameInput;
    private Button registerBtn;

    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Inicializa los modelos y elementos de la interfaz</para>
    /// </summary>
    private void Awake()
    {
        nameInput = GameObject.Find("NameInput").GetComponent<TMP_InputField>();
        registerBtn = GameObject.Find("RegisterBtn").GetComponent<Button>();
    }

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Asigna las acciones a los botones de la interfaz</para>
    /// </summary>
    private void Start()
    {
        registerBtn.onClick.AddListener(() => RegisterName(nameInput.text));
        nameInput.onSubmit.AddListener((name) => { if (IsValidNameInput()) RegisterName(name); });
    }

    /// <summary>
    /// Llamada al comienzo de cada fotograma
    /// <para>Evalua la entrada de texto</para>
    /// </summary>
    private void Update()
    {
        registerBtn.interactable = IsValidNameInput();
    }

    /// <summary>
    /// Comprueba si el nombre introducido es valido
    /// </summary>
    /// <returns>La validez del nombre introducido</returns>
    bool IsValidNameInput()
    {
        return nameInput.text != "";
    }

    /// <summary>
    /// Registra el nombre introducido del usuario actual
    /// </summary>
    /// <param name="name">Nombre introducido</param>
    public void RegisterName(string name)
    {
        if (UserManager.IsUserRegistered())
        {
            UserManager.ChangeUsername(name).ContinueWithOnMainThread(task =>
            {
                MySceneManager.LoadScene("Main");
            });
        }
        else
        {
            UserManager.SignUp(name).ContinueWithOnMainThread(task =>
            {
                MySceneManager.LoadScene("Main");
            });
        }
    }
}
