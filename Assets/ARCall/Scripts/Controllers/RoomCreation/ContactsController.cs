using System;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using VoxelBusters.EssentialKit;

/// <summary>
/// Controla la lista de contactos en la interfaz de Crear Videollamada
/// </summary>
public class ContactsController : MonoBehaviour
{
    public GameObject contactPrefab;
    public Sprite whatsappIcon;
    Transform scrollContent;
    IAddressBookContact[] contacts;

    PermissionCallbacks permissionCallback;

    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Inicializa los modelos y elementos de la interfaz</para>
    /// </summary>
    private void Awake()
    {
        permissionCallback = new PermissionCallbacks();
        permissionCallback.PermissionGranted += PermissionGranted;
        permissionCallback.PermissionDenied += PermissionDenied;
        permissionCallback.PermissionDeniedAndDontAskAgain += PermissionDeniedAndDontAskAgain;

        scrollContent = GameObject.Find("ScrollContent").transform;
    }

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Evalua los permisos de lectura de contactos</para>
    /// </summary>
    private void Start()
    {
        if (!String.IsNullOrEmpty(UserManager.CurrentUser.phoneNumber))
        {
            if (!Permission.HasUserAuthorizedPermission("android.permission.READ_CONTACTS"))
            {
                Permission.RequestUserPermission("android.permission.READ_CONTACTS", permissionCallback);
            }
            else
            {
                AddressBook.ReadContacts((result, error) =>
                {
                    if (error != null) Debug.LogError(error.Description);
                    ShowContacts(result.Contacts);
                });
            }
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// LLamada cuando se aceptan los permisos de lectura de contactos
    /// </summary>
    /// <param name="permissionName">Nombre del permiso aceptado</param>
    void PermissionGranted(string permissionName)
    {
        AddressBook.ReadContacts((result, error) =>
        {
            if (error != null) Debug.LogError(error.Description);
            ShowContacts(result.Contacts);
        });
    }

    /// <summary>
    /// LLamada cuando se deniegan los permisos de lectura de contactos
    /// </summary>
    /// <param name="permissionName">Nombre del permiso aceptado</param>
    void PermissionDenied(string permissionName)
    {
        Debug.LogWarning(permissionName + " permission denied");
    }

    /// <summary>
    /// LLamada cuando se deniegan los permisos de lectura de contactos y se solicita que no se vuelvan a pedir
    /// </summary>
    /// <param name="permissionName">Nombre del permiso aceptado</param>
    void PermissionDeniedAndDontAskAgain(string permissionName)
    {
        Debug.LogWarning(permissionName + " permission denied");
    }

    /// <summary>
    /// Inicializa todos los elementos necesarios de la lista de contactos y la muestra en la interfaz
    /// </summary>
    /// <param name="contacts">Array de contactos del sistema</param>
    void ShowContacts(IAddressBookContact[] contacts)
    {
        foreach (IAddressBookContact contact in contacts)
        {
            // Instanciamos prefab
            var contactLine = GameObject.Instantiate(contactPrefab, scrollContent).transform;
            contactLine.localScale = Vector3.one;

            // Asignamos nombre
            contactLine.Find("Nombre").GetComponent<TextMeshProUGUI>().text = contact.FirstName + " " + contact.LastName;

            // Enlazamos numero de telefono al boton
            contactLine.Find("Llamar").GetComponent<Button>().onClick.AddListener(async () =>
            {
                var phoneNumber = contact.PhoneNumbers[0].Replace(" ", string.Empty);
                phoneNumber = phoneNumber[0] == '+' ? phoneNumber : "+34" + phoneNumber;

                var userID = await DatabaseManager.GetUserID(phoneNumber);
                if (!String.IsNullOrEmpty(userID))
                {
                    SharingManager.SendNotification(userID);
                }
                else
                {
                    SharingManager.ShareRoomWhatsappContact(phoneNumber);
                }

                await RoomManager.JoinRoom(PeerType.Host);
            });
        }
    }
}
