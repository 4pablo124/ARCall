using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

public static class AuthManager
{
    public static FirebaseAuth Auth;

    private static string verificationId;

    public static bool IsUserRegistered(){
        return Auth.CurrentUser != null;
    }

    public static Task SignUp(string username){
        return Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread( async task => {
            if(task.IsCompleted) {
                await ChangeUsername(username);
                return;
            }

            if (task.IsCanceled) {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }

            if (task.IsFaulted) {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            };
        });
    }
    
    public static Task ChangeUsername(string username){
        UserProfile profile = new UserProfile();
        profile.DisplayName = username;
        return Auth.CurrentUser.UpdateUserProfileAsync(profile);
    }

    public static void ChangePhoneNumber(string phoneNumber){
        
    }


    public static void SendVerificationCode(string countryCode, string phoneNumber){
        PhoneAuthProvider.GetInstance(Auth).VerifyPhoneNumber(countryCode+phoneNumber, 120000, null,
            verificationCompleted: (credential) => {
                // Auto-sms-retrieval or instant validation has succeeded (Android only).
                // There is no need to input the verification code.
                // `credential` can be used instead of calling GetCredential().
                Debug.Log("Code retrieved");

                // await Auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task => {
                //     if(task.IsCompleted){
                //         UISceneNav.loadScene("Registro");
                //     }
                //     if(task.IsFaulted){
                //         Debug.LogError(task.Exception); 
                //     }
                // });
            },
            verificationFailed: (error) => {
                // The verification code was not sent.
                // `error` contains a human readable explanation of the problem.
                Debug.LogError(error);

            },
            codeSent: (id, token) => {
                // Verification code was successfully sent via SMS.
                // `id` contains the verification id that will need to passed in with
                // the code from the user when calling GetCredential().
                // `token` can be used if the user requests the code be sent again, to
                // tie the two requests together.

                verificationId = id;
                Debug.Log("Code Sent");
            },
            codeAutoRetrievalTimeOut: (id) => {
                // Called when the auto-sms-retrieval has timed out, based on the given
                // timeout parameter.
                // `id` contains the verification id of the request that timed out.
                Debug.Log("Code retireval timed out: "+ id);

        });
    }

    public static Task VerifyPhone(string code){
        Credential credential = PhoneAuthProvider.GetInstance(Auth).GetCredential(verificationId, code);
        return Auth.SignInWithCredentialAsync(credential);
    }
}
