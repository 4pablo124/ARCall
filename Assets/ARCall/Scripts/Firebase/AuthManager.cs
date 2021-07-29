using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public static class AuthManager
{

    public static event Action OnVerificationCompleted;
    public static event Action OnVerificationFailed;
    public static event Action OnCodeSent;
    public static event Action OnCodeAutoRetrievalTimeOut;

    public static FirebaseAuth Auth;

    private static string verificationId;

    public static bool IsUserRegistered(){
        return Auth.CurrentUser != null;
    }

    public static async Task<Task> SignUp(string username){
        await Auth.SignInAnonymouslyAsync();
        return ChangeUsername(username);
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
            verificationCompleted: async (credential) => {
                // Auto-sms-retrieval or instant validation has succeeded (Android only).
                // There is no need to input the verification code.
                // `credential` can be used instead of calling GetCredential().
                Debug.Log("Code retrieved");
                
                await VerifyPhoneCredential(credential);
                OnVerificationCompleted?.Invoke();
            },
            verificationFailed: (error) => {
                // The verification code was not sent.
                // `error` contains a human readable explanation of the problem.
                Debug.LogError(error);
                OnVerificationFailed?.Invoke();
            },
            codeSent: (id, token) => {
                // Verification code was successfully sent via SMS.
                // `id` contains the verification id that will need to passed in with
                // the code from the user when calling GetCredential().
                // `token` can be used if the user requests the code be sent again, to
                // tie the two requests together.

                verificationId = id;
                Debug.Log("Code Sent");
                OnCodeSent?.Invoke();
            },
            codeAutoRetrievalTimeOut: (id) => {
                // Called when the auto-sms-retrieval has timed out, based on the given
                // timeout parameter.
                // `id` contains the verification id of the request that timed out.
                Debug.Log("Code retireval timed out: "+ id);
                OnCodeAutoRetrievalTimeOut?.Invoke();
        });
    }

    public static async Task<bool> VerifyPhone(string code){
        try {
            Credential credential = PhoneAuthProvider.GetInstance(Auth).GetCredential(verificationId, code);
            await VerifyPhoneCredential(credential);
            return true;
        }
        catch (System.Exception e) {
            Debug.LogException(e);
            return false;   
        }
    }
    private static Task VerifyPhoneCredential(Credential credential){
        return Auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task => {
            var userID = OneSignal.GetPermissionSubscriptionState().subscriptionStatus.userId;
            return FirebaseDatabase.DefaultInstance.GetReference("UserIDs").Child(AuthManager.Auth.CurrentUser.PhoneNumber).SetValueAsync(userID);
        });
    }
}
