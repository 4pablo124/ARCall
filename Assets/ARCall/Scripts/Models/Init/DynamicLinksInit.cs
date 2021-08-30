using Firebase.DynamicLinks;
using System;
using UnityEngine;

/// <summary>
/// Inicializa los links dinámicos
/// </summary>
public class DynamicLinksInit : MonoBehaviour
{
    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Marca el <see cref="GameObject"/> asociado para que no sea destruido al cambiar de escena</para>
    /// </summary>
    private void Start()
    {
        DynamicLinks.DynamicLinkReceived += OnDynamicLink;
        DontDestroyOnLoad(this.gameObject);
    }

    // Display the dynamic link received by the application.
    /// <summary>
    /// Lanzada cuando se recibe un enlace dinámico
    /// <para>Redirige a la videollamda correspondiente</para>
    /// </summary>
    /// <param name="sender">Identifica el envio</param>
    /// <param name="args">argumentos del envio</param>
    private async void OnDynamicLink(object sender, EventArgs args)
    {

        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var intent = activity.Call<AndroidJavaObject>("getIntent");

        intent.Call("removeExtra", "com.google.firebase.dynamiclinks.DYNAMIC_LINK_DATA");
        intent.Call("removeExtra", "com.google.android.gms.appinvite.REFERRAL_BUNDLE");

        var dynamicLinkEventArgs = args as ReceivedDynamicLinkEventArgs;
        string url = dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString;

        Debug.LogFormat("Received dynamic link {0}", url);
        RoomManager.RoomID = url.Substring(url.LastIndexOf('/') + 1);
        if (!await RoomManager.JoinRoom(PeerType.Client))
        {
            AndroidUtils.ShowToast("La sala no existe o el Host no esta activo en este momento");
        };
    }

}
