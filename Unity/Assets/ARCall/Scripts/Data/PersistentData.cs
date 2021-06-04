using System;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public static class PersistentData
{
    private static String RoomID;

    public static void SetRoomID(String roomID){
        RoomID = roomID;
    }
    public static String GetRoomID(){
        return RoomID;
    }

    //Genera un codigo de sala
    public static async Task<String> GenerateRoomID(){
        int _min = 1000;
        int _max = 9999;
        var random = new System.Random();
        DataSnapshot snapshot;
        String roomID;

        do{
            roomID = random.Next(_min, _max).ToString();
            Debug.Log($"Evaluando codigo de sala: {roomID}");
            snapshot = await FirebaseDatabase.DefaultInstance.GetReference("Rooms").Child(roomID).GetValueAsync();

        }while(snapshot.Exists);
        Debug.Log($"Sala valida: {roomID}");
        
        RoomID = roomID;
        return roomID;
    }
}
