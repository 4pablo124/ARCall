using System;
using Firebase;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseInit : MonoBehaviour
{
    public static Firebase.FirebaseApp FirebaseApp;
    public static event Action OnReady;
    public static bool Ready;
    
    // Start is called before the first frame update
    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                FirebaseApp = FirebaseApp.DefaultInstance;
                AuthManager.Auth = Firebase.Auth.FirebaseAuth.GetAuth(FirebaseApp);
                if(AuthManager.IsUserRegistered()){
                    UISceneNav.LoadScene("Main");
                }else{
                    UISceneNav.LoadScene("RegistroTlf");
                }
                // Set a flag here to indicate whether Firebase is ready to use by your app.
                Ready = true;
                OnReady?.Invoke();
            } else {
                UnityEngine.Debug.LogError(System.String.Format(
                "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    
}
