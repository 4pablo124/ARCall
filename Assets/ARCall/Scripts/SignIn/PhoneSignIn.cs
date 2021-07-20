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
    public TMP_InputField phoneInput;
    public TMP_InputField codeInput;
    public Button sendButton;
    public Button verifyButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        sendButton.interactable = phoneInput.text.Length == 9 ? true : false;
        verifyButton.interactable = codeInput.text == "" ? false : true;
    }


    public void SendVerificationCode(){
        AuthManager.SendVerificationCode(CountryCode.Spain,phoneInput.text);
    }

    public void VerifyPhone(){
        try
        {
            AuthManager.VerifyPhone(codeInput.text).ContinueWithOnMainThread(task => {   
                if(task.IsCompleted){
                    UISceneNav.LoadScene("Registro");
                }
                if(task.IsFaulted){
                    Debug.LogError(task.Exception); 
                }
            });
            
        }
        catch (System.Exception e) { Debug.LogException(e);}
    }
}
