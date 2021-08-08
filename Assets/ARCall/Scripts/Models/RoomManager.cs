using System;
using System.Threading.Tasks;
using UnityEngine;

public static class RoomManager {
    public static string RoomID = "0000";
    //Genera un codigo de sala
    public static async Task<String> GenerateRoomID(){
        int _min = 1000;
        int _max = 9999;
        var random = new System.Random();
        string roomID;

        do{
            roomID = random.Next(_min, _max).ToString();
            Debug.Log($"Evaluando codigo de sala: {roomID}");
        }while(await DatabaseManager.RoomIDExists(roomID));
        
        Debug.Log($"Sala valida: {roomID}");
        
        RoomID = roomID; 
        return roomID;
    }

    public static async Task<bool> JoinRoom(PeerType peer){
        if(peer == PeerType.Host){
            Debug.Log($"Entrando en sala: {RoomID}");
            MySceneManager.LoadScene("Host");
            return true;
        }else{
            if(await DatabaseManager.RoomIDExists(RoomID)){
                MySceneManager.LoadScene("Client");
                return true;
            }else{
                return false;
            }
        }
    }
}
