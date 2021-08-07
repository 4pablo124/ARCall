using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{

    private Button createBtn;
    private Button joinBtn;
    private Button recBtn;
    private Button signOffBtn;
    private TextMeshProUGUI usernameText;

    private void Awake() {
        createBtn = GameObject.Find("CreateBtn").GetComponent<Button>();
        joinBtn = GameObject.Find("JoinBtn").GetComponent<Button>();
        recBtn = GameObject.Find("RecBtn").GetComponent<Button>();
        signOffBtn = GameObject.Find("SignOffBtn").GetComponent<Button>();

        // usernameText = GameObject.Find("Username").GetComponent<TextMeshProUGUI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        createBtn.onClick.AddListener(()=>{
            UISceneNav.LoadScene("CreateRoom");
        });
        
        joinBtn.onClick.AddListener(()=>{
            UISceneNav.LoadScene("JoinRoom");
        });

        recBtn.onClick.AddListener(()=>{
            UISceneNav.LoadScene("Record");
        });

        signOffBtn.onClick.AddListener(()=>{
            AuthManager.SignOut();
            UISceneNav.LoadScene("RegistroTlf");
        });
        
        // usernameText.text = "Usuario: " + AuthManager.Auth.CurrentUser.DisplayName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
