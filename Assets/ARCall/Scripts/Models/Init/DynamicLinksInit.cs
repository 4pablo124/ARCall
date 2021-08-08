using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.DynamicLinks;
using UnityEngine;

public class DynamicLinksInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DynamicLinks.DynamicLinkReceived += OnDynamicLink;
        DontDestroyOnLoad(this.gameObject);
    }

    // Display the dynamic link received by the application.
    private async void OnDynamicLink(object sender, EventArgs args) {

        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var intent = activity.Call<AndroidJavaObject>("getIntent");

        intent.Call("removeExtra", "com.google.firebase.dynamiclinks.DYNAMIC_LINK_DATA");
        intent.Call("removeExtra", "com.google.android.gms.appinvite.REFERRAL_BUNDLE");

        var dynamicLinkEventArgs = args as ReceivedDynamicLinkEventArgs;
        string url = dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString;
                        
        Debug.LogFormat("Received dynamic link {0}",url);
        RoomManager.RoomID = url.Substring(url.LastIndexOf('/') + 1);
        if(!await RoomManager.JoinRoom(PeerType.Client)){
            AndroidUtils.ShowToast("La sala no existe o el Host no esta activo en este momento");
        };
    }

}
