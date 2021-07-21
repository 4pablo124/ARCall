using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;

public class NameSetUp : MonoBehaviour
{

    public TMP_InputField nameInput;
    public Button registerButton;


    // Update is called once per frame
    void Update()
    {   
        registerButton.interactable = nameInput.text == "" ? false : true;
    }

    public void register(){
        if(AuthManager.IsUserRegistered()){
            AuthManager.ChangeUsername(nameInput.text).ContinueWithOnMainThread(task => {
                Debug.Log(AuthManager.Auth.CurrentUser.DisplayName);
                UISceneNav.LoadScene("Main");
            });
        }else{
            AuthManager.SignUp(nameInput.text).ContinueWithOnMainThread(task => {
                Debug.Log(AuthManager.Auth.CurrentUser.DisplayName);
                UISceneNav.LoadScene("Main");
            });
        }
    }
}
