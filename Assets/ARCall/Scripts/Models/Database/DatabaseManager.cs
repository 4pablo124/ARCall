using System.Threading.Tasks;
using Firebase.Database;

public static class DatabaseManager {
    public static FirebaseDatabase Database;


    public static async Task<bool> RoomIDExists(string roomID){
        var snapshot = await Database.GetReference("Rooms").Child(roomID).GetValueAsync();
        return snapshot.Exists;
    }

}