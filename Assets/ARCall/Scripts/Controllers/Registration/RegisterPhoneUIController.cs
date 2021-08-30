using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la interfaz de registro
/// </summary>
public class RegisterPhoneUIController : MonoBehaviour
{
    private TMP_InputField phoneInput;
    private TMP_InputField codeInput;
    private Button sendBtn;
    private Button verifyBtn;
    private Button skipBtn;

    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Inicializa los modelos y elementos de la interfaz</para>
    /// </summary>
    private void Awake()
    {
        phoneInput = GameObject.Find("PhoneInput").GetComponent<TMP_InputField>();
        codeInput = GameObject.Find("CodeInput").GetComponent<TMP_InputField>();
        sendBtn = GameObject.Find("SendBtn").GetComponent<Button>();
        verifyBtn = GameObject.Find("VerifyBtn").GetComponent<Button>();
        skipBtn = GameObject.Find("SkipBtn").GetComponent<Button>();
    }

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Asigna las acciones a los botones de la interfaz</para>
    /// </summary>
    private void Start()
    {
        UserManager.OnVerificationCompleted += OnVerificationCompleted;
        UserManager.OnVerificationFailed += OnVerificationFailed;
        UserManager.OnCodeSent += OnCodeSent;
        UserManager.OnCodeAutoRetrievalTimeOut += OnCodeAutoRetrievalTimeOut;

        sendBtn.onClick.AddListener(() => SendCode(phoneInput.text));
        phoneInput.onSubmit.AddListener((phone) => { if (IsValidPhoneInput()) SendCode(phone); });

        verifyBtn.onClick.AddListener(() => VerifyPhone(codeInput.text));
        codeInput.onSubmit.AddListener((code) => { if (IsValidCodeInput()) VerifyPhone(code); });

        skipBtn.onClick.AddListener(() =>
        {
            Unsubscribe();
            MySceneManager.LoadScene("RegisterName");
        });

    }

    /// <summary>
    /// Llamada al comienzo de cada fotograma
    /// <para>Evalua la entrada de texto</para>
    /// </summary>
    private void Update()
    {
        sendBtn.interactable = IsValidPhoneInput();
        verifyBtn.interactable = IsValidCodeInput();
    }

    /// <summary>
    /// Envia el código SMS al teléfono introducido
    /// </summary>
    /// <param name="phone">telefono introducido</param>
    void SendCode(string phone)
    {
        AndroidUtils.ShowToast("¡Enviando codigo!");
        UserManager.SendVerificationCode(CountryCodes.SPAIN, phone);
    }

    /// <summary>
    /// Verifica el teléfono introducido con el código SMS introducido
    /// </summary>
    /// <param name="code">Código SMS introducido</param>
    async void VerifyPhone(string code)
    {
        if (await UserManager.VerifyPhone(code))
        {
            Unsubscribe();
            MySceneManager.LoadScene("RegisterName");
        }
        else
        {
            AndroidUtils.ShowToast("¡Código Incorrecto!");
        }
    }

    /// <summary>
    /// Evalua si el teléfono introducido es válido
    /// </summary>
    /// <returns>La validez del teléfono introducido</returns>
    bool IsValidPhoneInput()
    {
        return phoneInput.text.Length == 9;
    }
    /// <summary>
    /// Evalua si el código SMS introducido es válido
    /// </summary>
    /// <returns>La validez del código introducido</returns>
    bool IsValidCodeInput()
    {
        return codeInput.text.Length == 6;
    }

    /// <summary>
    /// Llamada cuando se completa la verificación
    /// <para>Notifica al usuario, se desuscribe de todos los eventos y redirige a la escena <c>RegisterName</c></para>
    /// </summary>
    private void OnVerificationCompleted()
    {
        AndroidUtils.ShowToast("¡Verificación automatica completada!");

        Unsubscribe();
        MySceneManager.LoadScene("RegisterName");
    }

    
    /// <summary>
    /// Llamada cuando falla la verificación
    /// <para>Notifica al usuario</para>
    /// </summary>
    private void OnVerificationFailed()
    {
        AndroidUtils.ShowToast("¡Verificación fallida!");
    }

    /// <summary>
    /// Llamada cuando se envíaa el código de verificación SMS
    /// <para>Notifica al usuario</para>
    /// </summary>
    private void OnCodeSent()
    {
        AndroidUtils.ShowToast("¡Codigo enviado!");
    }

    /// <summary>
    /// Llamada cuando se captura automáticamente el código
    /// <para>Sin implementar</para>
    /// </summary>
    private void OnCodeAutoRetrievalTimeOut()
    {
        //Sin implementar
    }

    /// <summary>
    /// Se desuscribe de los eventos de clases estáticas
    /// </summary>
    private void Unsubscribe()
    {
        UserManager.OnVerificationCompleted -= OnVerificationCompleted;
        UserManager.OnVerificationFailed -= OnVerificationFailed;
        UserManager.OnCodeSent -= OnCodeSent;
        UserManager.OnCodeAutoRetrievalTimeOut -= OnCodeAutoRetrievalTimeOut;
    }
}
