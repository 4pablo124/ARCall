using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Firebase.DynamicLinks;
using Firebase.Extensions;
using UnityEngine;

public static class Sharing{

    public static void SendNotification(string userID){
        var notification = new Dictionary<string,object>();
        notification["headings"] = new Dictionary<string, string>() { 
            {"en", "Incoming call from "+AuthManager.Auth.CurrentUser.DisplayName},
            {"es", "Llamada entrante de "+AuthManager.Auth.CurrentUser.DisplayName}
        };
        
        notification["contents"] = new Dictionary<string, string>() {
            {"en", AuthManager.Auth.CurrentUser.DisplayName + " wants to invite you to the room: " + RoomManager.RoomID},
            {"es", AuthManager.Auth.CurrentUser.DisplayName + " quiere invitarle a la sala: " + RoomManager.RoomID}
        };


        notification["include_player_ids"] = new List<string>() { userID };

        notification["android_channel_id"] = "bc08d491-65bf-4ecb-9e46-8fd6ed85ca26";
        notification["priority"] = 10;

        notification["android_background_layout"] = new Dictionary<string,string>() {
            {"image","onesignal_bgimage_default_image"},
            {"headings_color","ffffffff"},
            {"contents_color","ffffffff"}
        };
        notification["large_icon"] = "ic_phone_call";
        notification["large_icon"] = "ic_phone_call";

        notification["android_group"] = "ARCall";

        // notification["buttons"] = new List<Dictionary<string,string>>() {
        //     new Dictionary<string,string>(){
        //         {"id","acceptCall"},
        //         {"text","Aceptar Llamada"}
        //     },
        //     new Dictionary<string,string>(){
        //         {"id","denyCall"},
        //         {"text","Cancelar"}
        //     }
        // };

        var lines = notification.Select(kvp => kvp.Key + ": " + kvp.Value.ToString());
        Debug.Log(string.Join(Environment.NewLine, lines));

        OneSignal.PostNotification(notification);
    }

    public static Task ShareRoom(){
        return CreateDynamicRoomLink().ContinueWithOnMainThread(task => {
            if (task.IsCanceled) {
                Debug.LogError("GetShortLinkAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("GetShortLinkAsync encountered an error: " + task.Exception);
                return;
            }

            // Short Link has been created.
            ShortDynamicLink dynamicLink = task.Result;
            Debug.LogFormat("Generated short link {0}", dynamicLink.Url);

            string message = ShareMessage(dynamicLink);

            new NativeShare().SetTitle("ARCall")
                             .SetText(message)
                             .Share();
        });
    }

    public static Task ShareRoomWhatsappContact(string phoneNumber){
        return CreateDynamicRoomLink().ContinueWithOnMainThread(task => {
            if (task.IsCanceled) {
                Debug.LogError("GetShortLinkAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("GetShortLinkAsync encountered an error: " + task.Exception);
                return;
            }

            // Short Link has been created.
            ShortDynamicLink dynamicLink = task.Result;
            Debug.LogFormat("Generated short link {0}", dynamicLink.Url);

            string message = ShareMessage(dynamicLink);
                                        
            string url = "https://api.whatsapp.com/send?phone="+ phoneNumber +"&text=" + WebUtility.UrlEncode(message);

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = context.Call<AndroidJavaObject>("getPackageManager");

            AndroidJavaClass Intent = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject i = new AndroidJavaObject("android.content.Intent");

            i.Call<AndroidJavaObject>("setAction", Intent.GetStatic<AndroidJavaObject>("ACTION_VIEW"));
            i.Call<AndroidJavaObject>("setPackage", "com.whatsapp");

            AndroidJavaClass Uri = new AndroidJavaClass("android.net.Uri");
            i.Call<AndroidJavaObject>("setData", Uri.CallStatic<AndroidJavaObject>("parse", url));

            if(i.Call<AndroidJavaObject>("resolveActivity",packageManager) != null){
                context.Call("startActivity", i);    
            }
        });
    }

    private static Task<ShortDynamicLink> CreateDynamicRoomLink(){
        var components = new DynamicLinkComponents(
            new Uri("https://arcall.web.app/call/"+RoomManager.RoomID), "https://arcall.page.link"){
                AndroidParameters = new AndroidParameters("com.skrl.ARCall")
            };

        var options = new Firebase.DynamicLinks.DynamicLinkOptions {
                PathLength = DynamicLinkPathLength.Unguessable
            };

        return Firebase.DynamicLinks.DynamicLinks.GetShortLinkAsync(components, options);
    }

    private static string ShareMessage(ShortDynamicLink dynamicLink){
        return  "ARCall\n" +
                "\n" +
                "Unete a la videollamada con el siguiente enlace: \n" +
                dynamicLink.Url.AbsoluteUri + "\n" +
                "\n" +
                "O introduciendo este código en la aplicación: " + RoomManager.RoomID;
    }
}