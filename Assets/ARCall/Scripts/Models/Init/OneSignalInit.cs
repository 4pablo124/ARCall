using System;
using System.Collections.Generic;
using UnityEngine;

public class OneSignalInit : MonoBehaviour {
	void Start () {
		// Uncomment this method to enable OneSignal Debugging log output 
		OneSignal.SetLogLevel(OneSignal.LOG_LEVEL.VERBOSE, OneSignal.LOG_LEVEL.NONE);
		
		// Replace 'YOUR_ONESIGNAL_APP_ID' with your OneSignal App ID.
		OneSignal.StartInit("cc0b0b01-491f-4b8e-8d0a-be4ec67d6034")
		.HandleNotificationOpened(OneSignalHandleNotificationOpened)
		.Settings(new Dictionary<string, bool>() {
			{ OneSignal.kOSSettingsAutoPrompt, false },
			{ OneSignal.kOSSettingsInAppLaunchURL, false } })
		.EndInit();
		
		OneSignal.inFocusDisplayType = OneSignal.OSInFocusDisplayOption.Notification;
		
		// iOS - Shows the iOS native notification permission prompt.
		//   - Instead we recomemnd using an In-App Message to prompt for notification 
		//     permission to explain how notifications are helpful to your users.
		OneSignal.PromptForPushNotificationsWithUserResponse(OneSignalPromptForPushNotificationsReponse);
  	}

    // Gets called when the player opens a OneSignal notification.
    private static void OneSignalHandleNotificationOpened(OSNotificationOpenedResult result) {
		if(FirebaseInit.Ready){
			HandleNotification(result);
		}else{
			FirebaseInit.OnReady += (database,auth) => HandleNotification(result);
		}
    }

	private static async void HandleNotification(OSNotificationOpenedResult result){
		string body = result.notification.payload.body;
		RoomManager.RoomID = body.Substring(body.LastIndexOf(' ') + 1);
		if(!await RoomManager.JoinRoom(PeerType.Client)){
			AndroidUtils.ShowToast("La sala no esta disponible en este momento");
		};

		FirebaseInit.OnReady -= (database,auth) => HandleNotification(result);
	}

    // iOS - Fires when the user anwser the notification permission prompt.
    private void OneSignalPromptForPushNotificationsReponse(bool accepted) {
        // Optional callback if you need to know when the user accepts or declines notification permissions.
    }
}
