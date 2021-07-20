using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Firebase.Database;
using Firebase.Extensions;
using OneSignalPush.MiniJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using VoxelBusters.EssentialKit;

public class ContactLoader : MonoBehaviour
{
    Transform scrollContent;
    public GameObject contactPrefab;
    IAddressBookContact[] contacts;

    PermissionCallbacks permissionCallback;

    private void Awake()
    {
        permissionCallback = new PermissionCallbacks();
        permissionCallback.PermissionGranted += PermissionGranted;
        permissionCallback.PermissionDenied += PermissionDenied;
        permissionCallback.PermissionDeniedAndDontAskAgain += PermissionDeniedAndDontAskAgain;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(!AuthManager.Auth.CurrentUser.IsAnonymous){
            if(!Permission.HasUserAuthorizedPermission("android.permission.READ_CONTACTS")){
                Permission.RequestUserPermission("android.permission.READ_CONTACTS", permissionCallback);
            }else{
                AddressBook.ReadContacts((result, error) => {
                if(error != null) Debug.LogError(error.Description);
                ShowContacts(result.Contacts);
            });
            }
        }else{
            transform.parent.gameObject.SetActive(false);
        }
    }


    void PermissionGranted(string permissionName){
        AddressBook.ReadContacts((result, error) => {
            if(error != null) Debug.LogError(error.Description);
            ShowContacts(result.Contacts);
        });
    }
    void PermissionDenied(string permissionName){
        Debug.LogWarning(permissionName + " permission denied");
    }
    void PermissionDeniedAndDontAskAgain(string permissionName){
        Debug.LogWarning(permissionName + " permission denied");
    }
 

    void ShowContacts(IAddressBookContact[] contacts){
        foreach (IAddressBookContact contact in contacts){
            var contactLine = GameObject.Instantiate(contactPrefab,transform).transform;
            contactLine.localScale = Vector3.one;

            // contact.LoadImage((textureData,error) => {
            //     Texture2D texture = textureData?.GetTexture();
            //     if(texture != null){
            //         contactLine.Find("Imagen").GetComponent<Image>().sprite =
            //             Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            //     }
            // });

            contactLine.Find("Nombre").GetComponent<TextMeshProUGUI>().text = contact.FirstName + " " + contact.LastName;

            //TODO: Dividir responsabilidades
            contactLine.Find("Llamar").GetComponent<Button>().onClick.AddListener(()=>{
                var phoneNumber = contact.PhoneNumbers[0].Replace(" ", string.Empty);
                phoneNumber = phoneNumber[0] == '+' ? phoneNumber : "+34" + phoneNumber;
                Debug.Log(phoneNumber);

                FirebaseDatabase.DefaultInstance.GetReference("UserIDs").Child(phoneNumber).GetValueAsync().ContinueWithOnMainThread(async task =>{
                    if(task.Result.Exists){
                        string userID = task.Result.Value.ToString();

                        var notification = new Dictionary<string,object>();
                        notification["headings"] = new Dictionary<string, string>() { {"en", "Llamada entrante de "+AuthManager.Auth.CurrentUser.DisplayName} };
                        notification["contents"] = new Dictionary<string, string>() { {"en", AuthManager.Auth.CurrentUser.DisplayName + " quiere invitarle a la sala: " + RoomManager.RoomID} };
                        notification["include_player_ids"] = new List<string>() { userID };
                        notification["android_channel_id"] = "bc08d491-65bf-4ecb-9e46-8fd6ed85ca26";
                        notification["android_background_layout"] = new Dictionary<string,string>() {{"headings_color","FFFF0000"}};
                        notification["buttons"] = new List<Dictionary<string,string>>() {
                            new Dictionary<string,string>(){{"id","acceptCall"},{"text","Aceptar"}},
                            new Dictionary<string,string>(){{"id","denyCall"},{"text","Cancelar"}}
                        };

                        var lines = notification.Select(kvp => kvp.Key + ": " + kvp.Value.ToString());
                            Debug.Log(string.Join(Environment.NewLine, lines));

                        OneSignal.PostNotification(notification);



                    }else{

                        string message = "Codigo de sala: " + RoomManager.RoomID;
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
                    }

                    await RoomManager.JoinRoom(PeerType.Host);
                });

            });

        }
    }
}
