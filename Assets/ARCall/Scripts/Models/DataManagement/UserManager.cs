using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Maneja la lógica de registro y autenticación de usuarios
/// </summary>
public static class UserManager
{
    /// <summary>
    /// Evento lanzado cuando se completa la verificación de teléfono
    /// </summary>
    public static event Action OnVerificationCompleted;
    /// <summary>
    /// Evento lanzado cuando falla la verificación de teléfono
    /// </summary>
    public static event Action OnVerificationFailed;
    /// <summary>
    /// Evento lanzado cuando se envia el código SMS de verificación
    /// </summary>
    public static event Action OnCodeSent;
    /// <summary>
    /// Evento lanzado cuando falla la captación automática del código SMS de verificación
    /// </summary>
    public static event Action OnCodeAutoRetrievalTimeOut;
    /// <summary>
    /// Usuario actualmente logueado
    /// </summary>
    public static User CurrentUser;
    /// <summary>
    /// Referencia al objeto de Autenticación
    /// </summary>
    public static FirebaseAuth Auth;
    private static string verificationId;


    /// <summary>
    /// Evalua si el usuario actual esta registrado
    /// </summary>
    /// <returns>Si el usuario esta registrado</returns>
    public static bool IsUserRegistered()
    {
        return Auth.CurrentUser != null || !String.IsNullOrEmpty(Auth.CurrentUser?.DisplayName);
    }

    /// <summary>
    /// Registra el nombre de usuario
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    /// <returns>Tarea asincrona esperable</returns>
    public static async Task<Task> SignUp(string username)
    {
        await Auth.SignInAnonymouslyAsync();
        return ChangeUsername(username);
    }

    /// <summary>
    /// Cierra la sesion del usario actualmente logueado
    /// </summary>
    public static async void LogOut()
    {
        if (Auth.CurrentUser != null)
        {
            if (!String.IsNullOrEmpty(Auth.CurrentUser.PhoneNumber))
            {
                await DatabaseManager.RemoveUserID(Auth.CurrentUser.PhoneNumber);
            }
        await Auth.CurrentUser.DeleteAsync();
        }
        Auth.SignOut();
        CurrentUser = new User();
        OneSignal.SetSubscription(false);
    }

    /// <summary>
    /// Loguea al usuario actual
    /// </summary>
    /// <param name="auth">Objeto de autorización</param>
    public static void LogIn(FirebaseAuth auth)
    {
        Auth = auth;
        if (IsUserRegistered())
        {
            CurrentUser = new User(Auth.CurrentUser.DisplayName, Auth.CurrentUser.PhoneNumber);
        }
        else
        {
            CurrentUser = new User();
        }
    }

    /// <summary>
    /// Cambia el nombre del usuario actual
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    /// <returns>Tarea asíncrona esperable</returns>
    public static Task ChangeUsername(string username)
    {
        UserProfile profile = new UserProfile();
        CurrentUser.username = profile.DisplayName = username;
        return Auth.CurrentUser.UpdateUserProfileAsync(profile);
    }

    /// <summary>
    /// Envia el código SMS de verificación y lanza los posibles eventos
    /// </summary>
    /// <param name="countryCode">Código de país del teléfono</param>
    /// <param name="phoneNumber">Número de teléfono</param>
    public static void SendVerificationCode(string countryCode, string phoneNumber)
    {

        CurrentUser.phoneNumber = phoneNumber;

        PhoneAuthProvider.GetInstance(Auth).VerifyPhoneNumber(countryCode + phoneNumber, 120000, null,
            verificationCompleted: async (credential) =>
            {
                // Auto-sms-retrieval or instant validation has succeeded (Android only).
                // There is no need to input the verification code.
                // `credential` can be used instead of calling GetCredential().
                Debug.Log("Code retrieved");

                await VerifyPhoneCredential(credential);
                OnVerificationCompleted?.Invoke();
            },
            verificationFailed: (error) =>
            {
                // The verification code was not sent.
                // `error` contains a human readable explanation of the problem.
                Debug.LogError(error);

                CurrentUser.phoneNumber = null;

                OnVerificationFailed?.Invoke();
            },
            codeSent: (id, token) =>
            {
                // Verification code was successfully sent via SMS.
                // `id` contains the verification id that will need to passed in with
                // the code from the user when calling GetCredential().
                // `token` can be used if the user requests the code be sent again, to
                // tie the two requests together.

                verificationId = id;
                Debug.Log("Code Sent");
                OnCodeSent?.Invoke();
            },
            codeAutoRetrievalTimeOut: (id) =>
            {
                // Called when the auto-sms-retrieval has timed out, based on the given
                // timeout parameter.
                // `id` contains the verification id of the request that timed out.
                Debug.Log("Code retireval timed out: " + id);
                OnCodeAutoRetrievalTimeOut?.Invoke();
            });
    }

    /// <summary>
    /// Verifica el número de teléfono con el código SMS de verificación
    /// </summary>
    /// <param name="code">Código SMS de verificación</param>
    /// <returns>Exito de la verificación</returns>
    public static async Task<bool> VerifyPhone(string code)
    {
        try
        {
            Credential credential = PhoneAuthProvider.GetInstance(Auth).GetCredential(verificationId, code);
            await VerifyPhoneCredential(credential);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    /// <summary>
    /// Verifica el número de teléfono con un objeto de credenciales
    /// </summary>
    /// <param name="credential">Objeto de credenciales</param>
    /// <returns>Tarea asíncrona esperable</returns>
    private static Task VerifyPhoneCredential(Credential credential)
    {
        return Auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            OneSignal.SetSubscription(true);
            var userID = OneSignal.GetPermissionSubscriptionState().subscriptionStatus.userId;
            return DatabaseManager.SetUserID(UserManager.Auth.CurrentUser.PhoneNumber, userID);
        });
    }
}
