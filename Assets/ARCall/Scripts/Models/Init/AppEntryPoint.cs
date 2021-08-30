using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

/// <summary>
/// Punto de entra de la aplicación
/// </summary>
public class AppEntryPoint : MonoBehaviour
{
    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Se realizan las incializaiónes a nivel de aplicación y suscripciones a eventos</para>
    /// </summary>
    void Awake()
    {
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        FirebaseInit.OnReady += OnFirebaseReady;
    }

    /// <summary>
    /// Lanzado cuando Firebase esta inicializado
    /// </summary>
    /// <param name="database">Referencia a base de datos</param>
    /// <param name="auth">REferencia a objeto de autenticación</param>
    void OnFirebaseReady(FirebaseDatabase database, FirebaseAuth auth)
    {

        DatabaseManager.Database = database;
        UserManager.LogIn(auth);

#if UNITY_ANDROID || UNITY_EDITOR
        if (UserManager.IsUserRegistered())
        {
            MySceneManager.LoadScene("Main");
        }
        else
        {
            MySceneManager.LoadScene("RegisterPhone");
        }
#else
            MySceneManager.LoadScene("JoinRoom");
#endif

    }

    /// <summary>
    /// Llamadada cuando se destruye el <see cref="GameObject"/> asociado
    /// <para>Se desuscribe de eventos de clases estaticas</para>
    /// </summary>
    private void OnDestroy()
    {
        FirebaseInit.OnReady -= OnFirebaseReady;
    }
}
