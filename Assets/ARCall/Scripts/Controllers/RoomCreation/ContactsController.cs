using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using VoxelBusters.EssentialKit;

public class ContactsController : MonoBehaviour
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
            // Instanciamos prefab
            var contactLine = GameObject.Instantiate(contactPrefab,transform).transform;
            contactLine.localScale = Vector3.one;

            // Asignamos nombre
            contactLine.Find("Nombre").GetComponent<TextMeshProUGUI>().text = contact.FirstName + " " + contact.LastName;

            // Enlazamos numero de telefono al boton
            contactLine.Find("Llamar").GetComponent<Button>().onClick.AddListener(()=>{
                var phoneNumber = contact.PhoneNumbers[0].Replace(" ", string.Empty);
                phoneNumber = phoneNumber[0] == '+' ? phoneNumber : "+34" + phoneNumber;

                FirebaseDatabase.DefaultInstance.GetReference("UserIDs").Child(phoneNumber).GetValueAsync().ContinueWithOnMainThread(async task =>{
                    if(task.Result.Exists){
                        Sharing.SendNotification(userID: task.Result.Value.ToString());
                    }else{
                        await Sharing.ShareRoomWhatsappContact(phoneNumber);
                    }

                    await RoomManager.JoinRoom(PeerType.Host);
                });

            });

        }
    }
}
