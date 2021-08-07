using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneSignIn : MonoBehaviour
{
    private TMP_InputField phoneInput;
    private TMP_InputField codeInput;
    private Button sendBtn;
    private Button verifyBtn;
    private Button skipBtn;



    private void Awake() {
        phoneInput = GameObject.Find("PhoneInput").GetComponent<TMP_InputField>();
        codeInput = GameObject.Find("CodeInput").GetComponent<TMP_InputField>();
        sendBtn = GameObject.Find("SendBtn").GetComponent<Button>();
        verifyBtn = GameObject.Find("VerifyBtn").GetComponent<Button>();
        skipBtn = GameObject.Find("SkipBtn").GetComponent<Button>();
    }

    // Start is called before the first frame update
    void Start()
    {
        AuthManager.OnVerificationCompleted += OnVerificationCompleted;
        AuthManager.OnVerificationFailed += OnVerificationFailed;
        AuthManager.OnCodeSent += OnCodeSent;
        AuthManager.OnCodeAutoRetrievalTimeOut += OnCodeAutoRetrievalTimeOut;

        sendBtn.onClick.AddListener(() => SendCode(phoneInput.text));
        phoneInput.onSubmit.AddListener((phone) => {if(IsValidPhoneInput()) SendCode(phone);});

        verifyBtn.onClick.AddListener(() => VerifyPhone(codeInput.text));
        codeInput.onSubmit.AddListener((code) => {if(IsValidCodeInput()) VerifyPhone(code);});

        skipBtn.onClick.AddListener(() => {
            Unsubscribe();
            UISceneNav.LoadScene("Registro");
        });

    }

    // Update is called once per frame
    void Update()
    {
        sendBtn.interactable = IsValidPhoneInput();
        verifyBtn.interactable = IsValidCodeInput();
    }

    void SendCode(string phone){
        // codeNotif.text = "Enviando codigo!";
        AndroidUtils.ShowToast("¡Enviando codigo!");
        AuthManager.SendVerificationCode(CountryCode.Spain,phone);
    }

    async void VerifyPhone(string code){
        // codeNotif.text = "Enviando codigo!";
        // AndroidUtils.ShowToast("¡Enviando codigo!");
        
        if( await AuthManager.VerifyPhone(codeInput.text) ){
            Unsubscribe();
            UISceneNav.LoadScene("Registro");
        }else{
            // codeNotif.text = "Codigo Incorrecto!";
            AndroidUtils.ShowToast("¡Código Incorrecto!");
        }
    }

    bool IsValidPhoneInput(){
        return phoneInput.text.Length == 9;
    }
    bool IsValidCodeInput(){
        return codeInput.text.Length == 6;
    }

    private void OnVerificationCompleted(){
        // codeNotif.text = "Verificación automatica completada!";
        AndroidUtils.ShowToast("¡Verificación automatica completada!");

        Unsubscribe();
        UISceneNav.LoadScene("Registro");
    }

    private void OnVerificationFailed(){
        // codeNotif.text = "Verificación fallida!";
        AndroidUtils.ShowToast("¡Verificación fallida!");

    }

    private void OnCodeSent(){
        // codeNotif.text = "Codigo enviado!";
        AndroidUtils.ShowToast("¡Codigo enviado!");

    }

    private void OnCodeAutoRetrievalTimeOut(){

    }

    private void Unsubscribe(){
        AuthManager.OnVerificationCompleted -= OnVerificationCompleted;
        AuthManager.OnVerificationFailed -= OnVerificationFailed;
        AuthManager.OnCodeSent -= OnCodeSent;
        AuthManager.OnCodeAutoRetrievalTimeOut -= OnCodeAutoRetrievalTimeOut;
    }
}
