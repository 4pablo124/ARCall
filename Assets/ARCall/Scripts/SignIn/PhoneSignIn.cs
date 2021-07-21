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

    private TextMeshProUGUI codeNotif;


    private void Awake() {
        phoneInput = GameObject.Find("PhoneInput").GetComponent<TMP_InputField>();
        codeInput = GameObject.Find("CodeInput").GetComponent<TMP_InputField>();
        sendBtn = GameObject.Find("SendBtn").GetComponent<Button>();
        verifyBtn = GameObject.Find("VerifyBtn").GetComponent<Button>();
        skipBtn = GameObject.Find("SkipBtn").GetComponent<Button>();
        codeNotif = GameObject.Find("CodeNotif").GetComponent<TextMeshProUGUI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        AuthManager.OnVerificationCompleted += OnVerificationCompleted;
        AuthManager.OnVerificationFailed += OnVerificationFailed;
        AuthManager.OnCodeSent += OnCodeSent;
        AuthManager.OnCodeAutoRetrievalTimeOut += OnCodeAutoRetrievalTimeOut;

        sendBtn.onClick.AddListener(() => {
            codeNotif.text = "Enviando codigo!";
            AuthManager.SendVerificationCode(CountryCode.Spain,phoneInput.text);
        });

        verifyBtn.onClick.AddListener(async () => {
            if( await AuthManager.VerifyPhone(codeInput.text) ){
                Unsubscribe();
                UISceneNav.LoadScene("Registro");
            }else{
                codeNotif.text = "Codigo Incorrecto!";
            }
        });

        skipBtn.onClick.AddListener(() => {
            Unsubscribe();
            UISceneNav.LoadScene("Registro");
        });
    }

    // Update is called once per frame
    void Update()
    {
        sendBtn.interactable = phoneInput.text.Length == 9 ? true : false;
        verifyBtn.interactable = codeInput.text.Length == 6 ? true : false;
    }

    private void OnVerificationCompleted(){
        codeNotif.text = "Verificación automatica completada!";
        Unsubscribe();
        UISceneNav.LoadScene("Registro");
    }

    private void OnVerificationFailed(){
        codeNotif.text = "Verificación fallida!";

    }

    private void OnCodeSent(){
        codeNotif.text = "Codigo enviado!";
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
