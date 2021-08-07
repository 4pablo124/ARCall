using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;

public class NameSetUp : MonoBehaviour
{

    private TMP_InputField nameInput;
    private Button registerBtn;


    private void Awake() {
        nameInput = GameObject.Find("NameInput").GetComponent<TMP_InputField>();
        registerBtn = GameObject.Find("RegisterBtn").GetComponent<Button>();
    }

    private void Start() {
        registerBtn.onClick.AddListener(() => RegisterName(nameInput.text));
        nameInput.onSubmit.AddListener((name) => {if(IsValidNameInput()) RegisterName(name);});
    }

    // Update is called once per frame
    void Update()
    {   
        registerBtn.interactable = IsValidNameInput();
    }


    bool IsValidNameInput(){
        return nameInput.text != "";
    }

    public void RegisterName(string name){
        if(AuthManager.IsUserRegistered()){
            AuthManager.ChangeUsername(name).ContinueWithOnMainThread(task => {
                Debug.Log(AuthManager.Auth.CurrentUser.DisplayName);
                UISceneNav.LoadScene("Main");
            });
        }else{
            AuthManager.SignUp(name).ContinueWithOnMainThread(task => {
                Debug.Log(AuthManager.Auth.CurrentUser.DisplayName);
                UISceneNav.LoadScene("Main");
            });
        }
    }
}
