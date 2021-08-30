using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Maneja la lógica de la creación de salas
/// </summary>
public static class RoomManager
{
    /// <summary>
    /// Código actual de la sala
    /// </summary>
    public static string RoomID = "0000";

    /// <summary>
    /// Genera un codigo de sala aleatorio
    /// </summary>
    /// <returns>Código de sala generado</returns>
    public static async Task<String> GenerateRoomID()
    {
        int _min = 1000;
        int _max = 9999;
        var random = new System.Random();
        string roomID;

        do
        {
            roomID = random.Next(_min, _max).ToString();
            Debug.Log($"Evaluando codigo de sala: {roomID}");
        } while (await DatabaseManager.RoomIDExists(roomID));

        Debug.Log($"Sala valida: {roomID}");

        RoomID = roomID;
        return roomID;
    }

    /// <summary>
    /// Mueve un par a una sala
    /// </summary>
    /// <param name="peer">Par</param>
    /// <returns>Exito de la unión a la sala</returns>
    public static async Task<bool> JoinRoom(PeerType peer)
    {
        if (peer == PeerType.Host)
        {
            Debug.Log($"Entrando en sala: {RoomID}");
            MySceneManager.LoadScene("Host");
            return true;
        }
        else
        {
            if (await DatabaseManager.RoomIDExists(RoomID))
            {
                MySceneManager.LoadScene("Client");
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
