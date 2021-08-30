using Firebase.DynamicLinks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Maneja la lógica de notifiaciónes y compartir salas
/// </summary>
public static class SharingManager
{
    /// <summary>
    /// Envia una notifiación
    /// </summary>
    /// <param name="userID">codigo de usuario para la notifiación</param>
    public static void SendNotification(string userID)
    {
        var notification = new Dictionary<string, object>();
        notification["headings"] = new Dictionary<string, string>() {
            {"en", "AR Call: Incoming call from "+UserManager.CurrentUser.username},
            {"es", "AR Call: Llamada entrante de "+UserManager.CurrentUser.username}
        };

        notification["contents"] = new Dictionary<string, string>() {
            {"en", UserManager.CurrentUser.username + " wants to invite you to the room: " + RoomManager.RoomID},
            {"es", UserManager.CurrentUser.username + " quiere invitarle a la sala: " + RoomManager.RoomID}
        };

        notification["include_player_ids"] = new List<string>() { userID };

        notification["android_channel_id"] = "bc08d491-65bf-4ecb-9e46-8fd6ed85ca26";
        notification["priority"] = 10;

        notification["android_background_layout"] = new Dictionary<string, string>() {
            {"image","onesignal_bgimage_default_image"},
            {"headings_color","ffffffff"},
            {"contents_color","ffffffff"}
        };
        notification["large_icon"] = "ic_phone_call";

        OneSignal.PostNotification(notification);
    }

    /// <summary>
    /// Comparte un código de sala
    /// </summary>
    public static async void ShareRoom()
    {
        var dynamicLink = await CreateDynamicRoomLink();

        Debug.LogFormat("Generated short link {0}", dynamicLink.Url);

        string message = ShareMessage(dynamicLink);
        new NativeShare().SetTitle("AR Call").SetText(message).Share();
    }

    /// <summary>
    /// Comparte un código de sala con un contacto de whatsapp
    /// </summary>
    /// <param name="phoneNumber"></param>
    public static async void ShareRoomWhatsappContact(string phoneNumber)
    {
        var dynamicLink = await CreateDynamicRoomLink();

        Debug.LogFormat("Generated short link {0}", dynamicLink.Url);

        string message = ShareMessage(dynamicLink);

        string url = "https://api.whatsapp.com/send?phone=" + phoneNumber + "&text=" + WebUtility.UrlEncode(message);

        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = context.Call<AndroidJavaObject>("getPackageManager");

        AndroidJavaClass Intent = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject i = new AndroidJavaObject("android.content.Intent");

        i.Call<AndroidJavaObject>("setAction", Intent.GetStatic<AndroidJavaObject>("ACTION_VIEW"));
        i.Call<AndroidJavaObject>("setPackage", "com.whatsapp");

        AndroidJavaClass Uri = new AndroidJavaClass("android.net.Uri");
        i.Call<AndroidJavaObject>("setData", Uri.CallStatic<AndroidJavaObject>("parse", url));

        if (i.Call<AndroidJavaObject>("resolveActivity", packageManager) != null)
        {
            context.Call("startActivity", i);
        }
    }

    // Private Methods

    /// <summary>
    /// Crea un enlace dinámico a una sala
    /// </summary>
    /// <returns>Enlace dinámico</returns>
    private static Task<ShortDynamicLink> CreateDynamicRoomLink()
    {
        var components = new DynamicLinkComponents(
            new Uri("https://arcall.web.app/call/" + RoomManager.RoomID), "https://arcall.page.link")
        {
            AndroidParameters = new AndroidParameters("com.skrl.ARCall")
        };

        var options = new Firebase.DynamicLinks.DynamicLinkOptions
        {
            PathLength = DynamicLinkPathLength.Unguessable
        };

        return Firebase.DynamicLinks.DynamicLinks.GetShortLinkAsync(components, options);
    }

    /// <summary>
    /// Crea el mensaje a compartir
    /// </summary>
    /// <param name="dynamicLink">Enlace dinámico</param>
    /// <returns>Mensaje a compartir</returns>
    private static string ShareMessage(ShortDynamicLink dynamicLink)
    {
        return "AR Call\n" +
                "\n" +
                "Unete a la videollamada con el siguiente enlace: \n" +
                dynamicLink.Url.AbsoluteUri + "\n" +
                "\n" +
                "O introduce este código en la aplicación: " + RoomManager.RoomID;
    }
}