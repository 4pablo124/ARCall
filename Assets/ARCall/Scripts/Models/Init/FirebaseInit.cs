using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using UnityEngine;

/// <summary>
/// Inicializa Firebase
/// </summary>
public class FirebaseInit : MonoBehaviour
{
    /// <summary>
    /// Referencia a la aplicación de Firebase
    /// </summary>
    public static Firebase.FirebaseApp FirebaseApp;
    /// <summary>
    /// Evento lanzado cuando Firebase esta inicializado
    /// </summary>
    public static event Action<FirebaseDatabase, FirebaseAuth> OnReady;
    /// <summary>
    /// Indica si Firebase esta inicializado
    /// </summary>
    public static bool Ready;

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Inicializa Firebase</para>
    /// </summary>
    private void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                FirebaseApp = FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
                Ready = true;
                OnReady?.Invoke(Firebase.Database.FirebaseDatabase.DefaultInstance, Firebase.Auth.FirebaseAuth.GetAuth(FirebaseApp));
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }


}
