using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPhoneUIController : MonoBehaviour
{
    private TMP_InputField phoneInput;
    private TMP_InputField codeInput;
    private Button sendBtn;
    private Button verifyBtn;
    private Button skipBtn;



    private void Awake()
    {
        phoneInput = GameObject.Find("PhoneInput").GetComponent<TMP_InputField>();
        codeInput = GameObject.Find("CodeInput").GetComponent<TMP_InputField>();
        sendBtn = GameObject.Find("SendBtn").GetComponent<Button>();
        verifyBtn = GameObject.Find("VerifyBtn").GetComponent<Button>();
        skipBtn = GameObject.Find("SkipBtn").GetComponent<Button>();
    }

    // Start is called before the first frame update
    void Start()
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

    // Update is called once per frame
    void Update()
    {
        sendBtn.interactable = IsValidPhoneInput();
        verifyBtn.interactable = IsValidCodeInput();
    }

    void SendCode(string phone)
    {
        AndroidUtils.ShowToast("¡Enviando codigo!");
        UserManager.SendVerificationCode(CountryCodes.SPAIN, phone);
    }

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

    bool IsValidPhoneInput()
    {
        return phoneInput.text.Length == 9;
    }
    bool IsValidCodeInput()
    {
        return codeInput.text.Length == 6;
    }

    private void OnVerificationCompleted()
    {
        AndroidUtils.ShowToast("¡Verificación automatica completada!");

        Unsubscribe();
        MySceneManager.LoadScene("RegisterName");
    }

    private void OnVerificationFailed()
    {
        AndroidUtils.ShowToast("¡Verificación fallida!");

    }

    private void OnCodeSent()
    {
        AndroidUtils.ShowToast("¡Codigo enviado!");

    }

    private void OnCodeAutoRetrievalTimeOut()
    {

    }

    private void Unsubscribe()
    {
        UserManager.OnVerificationCompleted -= OnVerificationCompleted;
        UserManager.OnVerificationFailed -= OnVerificationFailed;
        UserManager.OnCodeSent -= OnCodeSent;
        UserManager.OnCodeAutoRetrievalTimeOut -= OnCodeAutoRetrievalTimeOut;
    }
}
