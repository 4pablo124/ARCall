// using System.Collections;
// using System.Collections.Generic;
// using Firebase.Database;
// using Firebase.Extensions;
// using NUnit.Framework;
// using TMPro;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.TestTools;

// public class SalasTestSuite
// {
//     [UnityTest]
//     public IEnumerator GenerateRoomIDGeneraCodigosDeSalaAleatorios()
//     {
//         string roomID;
//         var task = PersistentData.GenerateRoomID();
//         yield return new WaitUntil(()=>task.IsCompleted);

//         roomID=task.Result;

//         var task2 = PersistentData.GenerateRoomID();
//         yield return new WaitUntil(()=>task2.IsCompleted);

//         Assert.AreNotEqual(roomID,task2.Result);
//     }

//     [UnityTest]
//     public IEnumerator JoinRoomCreaSalaComoHost(){
//         RoomLoader roomManager = new RoomLoader();
//         roomManager.peerType = PeerType.Host;
//         //Sala 0000 no existe como sala real luego la podemos usar para pruebas
//         RoomLoader.roomID = "0000";
//         roomManager.JoinRoom();
//         yield return new WaitForSeconds(0.5f);
//         var task = FirebaseDatabase.DefaultInstance.GetReference("Rooms/0000/Host/Ready").GetValueAsync();
//         yield return new WaitUntil(()=>task.IsCompleted);
//         Assert.True(task.Result.Exists);
//         var task2 = FirebaseDatabase.DefaultInstance.GetReference("Rooms/0000/Host").RemoveValueAsync();
//         yield return new WaitUntil(()=>task2.IsCompleted);

//     }

//     [UnityTest]
//     public IEnumerator JoinRoomSeUneASalaComoCliente(){
//         RoomLoader roomManager = new RoomLoader();
//         roomManager.peerType = PeerType.Client;
//         GameObject errorObj = new GameObject();
//         errorObj.AddComponent<TextMeshProUGUI>();
//         roomManager.errorText = errorObj.GetComponent<TextMeshProUGUI>();
//         //Sala 0000 no existe como sala real luego la podemos usar para pruebas
//         RoomLoader.roomID = "0000";
//         var task = FirebaseDatabase.DefaultInstance.GetReference("Rooms/0000/Host/Ready").SetValueAsync(true);
//         yield return new WaitUntil(()=>task.IsCompleted);
//         roomManager.JoinRoom();
//         yield return new WaitForSeconds(0.5f);
//         var task2 = FirebaseDatabase.DefaultInstance.GetReference("Rooms/0000/Client/Ready").GetValueAsync();
//         yield return new WaitUntil(()=>task2.IsCompleted);
//         Assert.True(task2.Result.Exists);
//         var task3 = FirebaseDatabase.DefaultInstance.GetReference("Rooms/0000/Client").RemoveValueAsync();
//         yield return new WaitUntil(()=>task3.IsCompleted);
//     }

//     [UnityTest]
//     public IEnumerator JoinRoomMuestraErrorSiSalaNoExiste(){
//         RoomLoader roomManager = new RoomLoader();
//         roomManager.peerType = PeerType.Client;
//         GameObject errorObj = new GameObject();
//         errorObj.SetActive(false);
//         errorObj.AddComponent<TextMeshProUGUI>();
//         roomManager.errorText = errorObj.GetComponent<TextMeshProUGUI>();
//         //Sala 0000 no existe como sala real luego la podemos usar para pruebas
//         RoomLoader.roomID = "0000";
//         roomManager.JoinRoom();
//         yield return new WaitForSeconds(0.5f);
//         Assert.True(errorObj.activeSelf);
//     }

    
// }
