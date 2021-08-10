using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;

public class RegisterNameUIController : MonoBehaviour
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
        if(UserManager.IsUserRegistered()){
            UserManager.ChangeUsername(name).ContinueWithOnMainThread(task => {
                MySceneManager.LoadScene("Main");
            });
        }else{
            UserManager.SignUp(name).ContinueWithOnMainThread(task => {
                MySceneManager.LoadScene("Main");
            });
        }
    }
}
