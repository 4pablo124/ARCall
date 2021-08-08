using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

public class AppEntryPoint : MonoBehaviour
{
    void Awake(){
        Application.targetFrameRate = Screen.currentResolution.refreshRate;

        FirebaseInit.OnReady += OnFirebaseReady;
    }

    void OnFirebaseReady(FirebaseDatabase database, FirebaseAuth auth){

        DatabaseManager.Database = database;
        UserManager.LogIn(auth);

        #if UNITY_ANDROID || UNITY_EDITOR
            if(UserManager.IsUserRegistered()){
                MySceneManager.LoadScene("Main");
            }else{
                MySceneManager.LoadScene("RegistroTlf");
            }
        #else
            UISceneNav.LoadScene("JoinRoom");
        #endif

    }

    private void OnDestroy() {
        FirebaseInit.OnReady -= OnFirebaseReady;
    }
}
