using System;
using System.Threading.Tasks;
using Firebase.Database;
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

    public static void ReadyUser(string roomID, PeerType peer){
        Database.GetReference("Rooms").Child(roomID).Child(peer.ToString()).Child("Ready").SetValueAsync(true).
        ContinueWith(task => {
            if(peer == PeerType.Host) {
                Database.GetReference("Rooms").Child(roomID).Child("Client").ChildAdded += OnClientReadyDelegate;
            }

            Database.GetReference("Rooms").Child(roomID).Child("Messages").ChildAdded += OnMessageReceivedDelegate;

        });
    }

    public static void UnReadyUser(string roomID, PeerType peer){
        Database.GetReference("Rooms").Child(roomID).Child(peer.ToString()).Child("Ready").RemoveValueAsync().
        ContinueWith(task => {
            if(peer == PeerType.Host) {
                Database.GetReference("Rooms").Child(roomID).Child("Client").ChildAdded -= OnClientReadyDelegate;
            }

            Database.GetReference("Rooms").Child(roomID).Child("Messages").ChildAdded -= OnMessageReceivedDelegate;

        });
    }

    private static void OnClientReadyDelegate(Object sender, ChildChangedEventArgs args){
        OnClientReady?.Invoke();
    }

    private static void OnMessageReceivedDelegate(Object sender, ChildChangedEventArgs args){
        var msg = JsonConvert.DeserializeObject<Message>(args.Snapshot.GetRawJsonValue());
        args.Snapshot.Reference.RemoveValueAsync();
        OnMessageReceived?.Invoke(msg);
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
        return snapshot.GetRawJsonValue();
    }

    public static Task SetUserID(string phoneNumber, string userID){
        return Database.GetReference("UserIDs").Child(phoneNumber).SetValueAsync(userID);
    }
}
