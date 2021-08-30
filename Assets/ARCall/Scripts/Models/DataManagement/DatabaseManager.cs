using Firebase.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

/// <summary>
/// Maneja la lógica de la base de datos
/// </summary>
public static class DatabaseManager
{
    /// <summary>
    /// Referencia a la base de datos
    /// </summary>
    public static FirebaseDatabase Database;
    /// <summary>
    /// Evento lanzado cuando el cliente esta listo
    /// </summary>
    public static event Action OnClientReady;
    /// <summary>
    /// Evento lanzado cuando se recibe un mensaje de base de datos
    /// </summary>
    public static event Action<Message> OnMessageReceived;

    /// <summary>
    /// Evalua si un código de sala existe en base de datos
    /// </summary>
    /// <param name="roomID">Código de sala</param>
    /// <returns>La existencia del código de sala en base de datos</returns>
    public static async Task<bool> RoomIDExists(string roomID)
    {
        var snapshot = await Database.GetReference("Rooms").Child(roomID).GetValueAsync();
        return snapshot.Exists;
    }

    /// <summary>
    /// Señaliza en base de datos que un par esta listo para la conexión
    /// </summary>
    /// <param name="roomID">Sala a la que el par se conecta</param>
    /// <param name="peer">Rol del par</param>
    /// <returns></returns>
    public static async Task ReadyUser(string roomID, PeerType peer)
    {
        await Database.GetReference("Rooms").Child(roomID).Child(peer.ToString()).Child("Ready").SetValueAsync(true);

        if (peer == PeerType.Host)
        {
            Database.GetReference("Rooms").Child(roomID).Child("Client").ChildAdded += OnClientReadyDelegate;
        }

        Database.GetReference("Rooms").Child(roomID).Child("Messages").ChildAdded += OnMessageReceivedDelegate;
    }

    /// <summary>
    /// Señaliza en base de datos que un par no listo para la conexión
    /// </summary>
    /// <param name="roomID">Sala a la que el par se desconecta</param>
    /// <param name="peer">Rol del par</param>
    /// <returns></returns>
    public static async Task UnReadyUser(string roomID, PeerType peer)
    {
        await Database.GetReference("Rooms").Child(roomID).Child(peer.ToString()).Child("Ready").RemoveValueAsync();

        if (peer == PeerType.Host)
        {
            Database.GetReference("Rooms").Child(roomID).Child("Client").ChildAdded -= OnClientReadyDelegate;
        }

        Database.GetReference("Rooms").Child(roomID).Child("Messages").ChildAdded -= OnMessageReceivedDelegate;
    }

    /// <summary>
    /// Envia un mensaje de conexión a base de datos
    /// </summary>
    /// <param name="roomID">Sala la que se envia el mensaje</param>
    /// <param name="peerType">Rol del par</param>
    /// <param name="data">Datos a enviar</param>
    public static void SendMessage(string roomID, PeerType peerType, Data data)
    {
        JObject messageJSON = JObject.FromObject(
            new Message { peerType = peerType, data = data }
        );

        Database.GetReference("Rooms").Child(roomID).Child("Messages").Push().SetRawJsonValueAsync(
            messageJSON.ToString()
        );
    }

    /// <summary>
    /// Obtiene el <c>userID</c> de un usuario a partir de su número de teléfono
    /// </summary>
    /// <param name="phoneNumber">Teléfono del usuario</param>
    /// <returns>userID del usuario</returns>
    public static async Task<string> GetUserID(string phoneNumber)
    {
        var snapshot = await Database.GetReference("UserIDs").Child(phoneNumber).GetValueAsync();
        if (snapshot.Exists)
        {
            return snapshot.GetRawJsonValue().Trim('"');
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Establece el userID de un usuario
    /// </summary>
    /// <param name="phoneNumber">Teléfono del usuario</param>
    /// <param name="userID">userID del usuario</param>
    /// <returns>Tarea asincrona esperable</returns>
    public static Task SetUserID(string phoneNumber, string userID)
    {
        return Database.GetReference("UserIDs").Child(phoneNumber).SetValueAsync(userID);
    }

    /// <summary>
    /// Elimina el userID de un usuario
    /// </summary>
    /// <param name="phoneNumber">Teléfono del usuario</param>
    /// <param name="userID">userID del usuario</param>
    /// <returns>Tarea asincrona esperable</returns>
    public static Task RemoveUserID(string phoneNumber)
    {
        return Database.GetReference("UserIDs").Child(phoneNumber).RemoveValueAsync();
    }

    // Private Methods

    /// <summary>
    /// Lanza el evento <see cref="OnClientReady"/>
    /// </summary>
    /// <param name="sender">Parametro de base de datos identificando al envio</param>
    /// <param name="args">Parametro de base de datos con argumentos de la acción</param>
    private static void OnClientReadyDelegate(Object sender, ChildChangedEventArgs args)
    {
        OnClientReady?.Invoke();
    }

    /// <summary>
    /// Lanza el evento <see cref="OnMessageReceived"/>
    /// </summary>
    /// <param name="sender">Parametro de base de datos identificando al envio</param>
    /// <param name="args">Parametro de base de datos con argumentos de la acción</param>
    private static void OnMessageReceivedDelegate(Object sender, ChildChangedEventArgs args)
    {
        var msg = JsonConvert.DeserializeObject<Message>(args.Snapshot.GetRawJsonValue());
        args.Snapshot.Reference.RemoveValueAsync();
        OnMessageReceived?.Invoke(msg);
    }
}
