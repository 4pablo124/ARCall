using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
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
        if(!Permission.HasUserAuthorizedPermission("android.permission.READ_CONTACTS")){
            Permission.RequestUserPermission("android.permission.READ_CONTACTS", permissionCallback);
        }else{
            AddressBook.ReadContacts((result, error) => {
            if(error != null) Debug.LogError(error.Description);
            ShowContacts(result.Contacts);
        });
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

            Debug.Log(contact.FirstName);

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
        }
    }
}
