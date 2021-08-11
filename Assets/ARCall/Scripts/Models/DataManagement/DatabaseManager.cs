using System;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class DatabaseManager {
    public static FirebaseDatabase Database;
    public static event Action OnClientReady;
    public static event Action<Message> OnMessageReceived;

    public static async Task<bool> RoomIDExists(string roomID){
        var snapshot = await Database.GetReference("Rooms").Child(roomID).GetValueAsync();
        return snapshot.Exists;
    }

    public static async Task ReadyUser(string roomID, PeerType peer){
        await Database.GetReference("Rooms").Child(roomID).Child(peer.ToString()).Child("Ready").SetValueAsync(true);
        
        if(peer == PeerType.Host) {
            Database.GetReference("Rooms").Child(roomID).Child("Client").ChildAdded += OnClientReadyDelegate;
        }

        Database.GetReference("Rooms").Child(roomID).Child("Messages").ChildAdded += OnMessageReceivedDelegate;
    }

    public static async Task UnReadyUser(string roomID, PeerType peer){
        await Database.GetReference("Rooms").Child(roomID).Child(peer.ToString()).Child("Ready").RemoveValueAsync();

        if(peer == PeerType.Host) {
            Database.GetReference("Rooms").Child(roomID).Child("Client").ChildAdded -= OnClientReadyDelegate;
        }

        Database.GetReference("Rooms").Child(roomID).Child("Messages").ChildAdded -= OnMessageReceivedDelegate;
    }

    public static void SendMessage(string roomID, PeerType peerType, Data data){
        JObject messageJSON = JObject.FromObject(
            new Message{ peerType = peerType, data = data }
        );
        
        Database.GetReference("Rooms").Child(roomID).Child("Messages").Push().SetRawJsonValueAsync(
            messageJSON.ToString()
        );
    }

    public static async Task<string> GetUserID(string phoneNumber){
        var snapshot = await Database.GetReference("UserIDs").Child(phoneNumber).GetValueAsync();
        return snapshot.GetRawJsonValue().Trim('"');
    }

    public static Task SetUserID(string phoneNumber, string userID){
        return Database.GetReference("UserIDs").Child(phoneNumber).SetValueAsync(userID);
    }

    public static Task RemoveUserID(string phoneNumber){
        return Database.GetReference("UserIDs").Child(phoneNumber).RemoveValueAsync();
    }

    // Private Methods
    private static void OnClientReadyDelegate(Object sender, ChildChangedEventArgs args){
        OnClientReady?.Invoke();
    }

    private static void OnMessageReceivedDelegate(Object sender, ChildChangedEventArgs args){
        var msg = JsonConvert.DeserializeObject<Message>(args.Snapshot.GetRawJsonValue());
        args.Snapshot.Reference.RemoveValueAsync();
        OnMessageReceived?.Invoke(msg);
    }
}
