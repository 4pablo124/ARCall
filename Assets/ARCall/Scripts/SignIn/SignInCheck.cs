using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;

public class SignInCheck : MonoBehaviour
{
    private TextMeshProUGUI usernameText;


    private void Awake() {
        usernameText = GameObject.Find("Username").GetComponent<TextMeshProUGUI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        usernameText.text = "Usuario: " + AuthManager.Auth.CurrentUser?.DisplayName;
    }

    public void SignOut(){

        if(AuthManager.Auth.CurrentUser.IsAnonymous){
            AuthManager.Auth.CurrentUser.DeleteAsync();
        }
        AuthManager.Auth.SignOut();
        UISceneNav.loadScene("Init");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
