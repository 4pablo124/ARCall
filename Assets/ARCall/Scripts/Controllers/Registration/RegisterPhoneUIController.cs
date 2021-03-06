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
    /// Envia el c??digo SMS al tel??fono introducido
    /// </summary>
    /// <param name="phone">telefono introducido</param>
    void SendCode(string phone)
    {
        AndroidUtils.ShowToast("??Enviando codigo!");
        UserManager.SendVerificationCode(CountryCodes.SPAIN, phone);
    }

    /// <summary>
    /// Verifica el tel??fono introducido con el c??digo SMS introducido
    /// </summary>
    /// <param name="code">C??digo SMS introducido</param>
    async void VerifyPhone(string code)
    {
        if (await UserManager.VerifyPhone(code))
        {
            Unsubscribe();
            MySceneManager.LoadScene("RegisterName");
        }
        else
        {
            AndroidUtils.ShowToast("??C??digo Incorrecto!");
        }
    }

    /// <summary>
    /// Evalua si el tel??fono introducido es v??lido
    /// </summary>
    /// <returns>La validez del tel??fono introducido</returns>
    bool IsValidPhoneInput()
    {
        return phoneInput.text.Length == 9;
    }
    /// <summary>
    /// Evalua si el c??digo SMS introducido es v??lido
    /// </summary>
    /// <returns>La validez del c??digo introducido</returns>
    bool IsValidCodeInput()
    {
        return codeInput.text.Length == 6;
    }

    /// <summary>
    /// Llamada cuando se completa la verificaci??n
    /// <para>Notifica al usuario, se desuscribe de todos los eventos y redirige a la escena <c>RegisterName</c></para>
    /// </summary>
    private void OnVerificationCompleted()
    {
        AndroidUtils.ShowToast("??Verificaci??n automatica completada!");

        Unsubscribe();
        MySceneManager.LoadScene("RegisterName");
    }

    
    /// <summary>
    /// Llamada cuando falla la verificaci??n
    /// <para>Notifica al usuario</para>
    /// </summary>
    private void OnVerificationFailed()
    {
        AndroidUtils.ShowToast("??Verificaci??n fallida!");
    }

    /// <summary>
    /// Llamada cuando se env??aa el c??digo de verificaci??n SMS
    /// <para>Notifica al usuario</para>
    /// </summary>
    private void OnCodeSent()
    {
        AndroidUtils.ShowToast("??Codigo enviado!");
    }

    /// <summary>
    /// Llamada cuando se captura autom??ticamente el c??digo
    /// <para>Sin implementar</para>
    /// </summary>
    private void OnCodeAutoRetrievalTimeOut()
    {
        //Sin implementar
    }

    /// <summary>
    /// Se desuscribe de los eventos de clases est??ticas
    /// </summary>
    private void Unsubscribe()
    {
        UserManager.OnVerificationCompleted -= OnVerificationCompleted;
        UserManager.OnVerificationFailed -= OnVerificationFailed;
        UserManager.OnCodeSent -= OnCodeSent;
        UserManager.OnCodeAutoRetrievalTimeOut -= OnCodeAutoRetrievalTimeOut;
    }
}
