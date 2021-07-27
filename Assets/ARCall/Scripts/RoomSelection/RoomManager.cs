using System;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public static class RoomManager {
    public static string RoomID = "0000"; //TODO: Temporal
    
    //Genera un codigo de sala
    public static async Task<String> GenerateRoomID(){
        int _min = 1000;
        int _max = 9999;
        var random = new System.Random();
        DataSnapshot snapshot;
        string roomID;

        do{
            roomID = random.Next(_min, _max).ToString();
            Debug.Log($"Evaluando codigo de sala: {roomID}");
            snapshot = await FirebaseDatabase.DefaultInstance.GetReference("Rooms").Child(roomID).GetValueAsync();
        }while(snapshot.Exists);
        
        Debug.Log($"Sala valida: {roomID}");
        
        RoomID = roomID; 
        return roomID;
    }

    public static async Task<bool> JoinRoom(PeerType peer){
        if(peer == PeerType.Host){
            Debug.Log($"Entrando en sala: {RoomID}");
            UISceneNav.LoadScene("Host");
            return true;
        }else{
            var snapshot = await FirebaseDatabase.DefaultInstance.GetReference("Rooms").Child(RoomID).GetValueAsync();
            if(snapshot.Exists){
                UISceneNav.LoadScene("Client");
                return true;
            }else{
                return false;
            }
        }
    }
}
