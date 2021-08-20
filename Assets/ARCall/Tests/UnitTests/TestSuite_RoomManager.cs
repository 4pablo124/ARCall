using NUnit.Framework;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;


public class TestSuite_RoomManager : TestDependenciesSetUp
{
    [UnityOneTimeTearDown]
    public IEnumerator UnityOneTimeTearDown()
    {
        yield return new WaitForSecondsRealtime(1);
    }

    [AsyncTest]
    public async Task GenerateRoomID_GeneratesRandomRoomID()
    {
        var roomID1 = await RoomManager.GenerateRoomID();
        var roomID2 = await RoomManager.GenerateRoomID();
        Assert.AreNotEqual(roomID1, roomID2);
    }

    [AsyncTest]
    public async Task JoinRoom_HostCreatesRoom()
    {
        RoomManager.RoomID = "0001";
        Assert.IsTrue(await RoomManager.JoinRoom(PeerType.Host));

        await Task.Delay(100);

        await DatabaseManager.UnReadyUser("0001", PeerType.Host);
    }
    [AsyncTest]
    public async Task JoinRoom_ClientJoinsValidRoom()
    {
        await DatabaseManager.ReadyUser("0002", PeerType.Host);

        RoomManager.RoomID = "0002";
        Assert.IsTrue(await RoomManager.JoinRoom(PeerType.Client));

        await DatabaseManager.UnReadyUser("0002", PeerType.Host);
        await DatabaseManager.UnReadyUser("0002", PeerType.Client);
    }
    [AsyncTest]
    public async Task JoinRoom_ClientDoesntJoinInvalidRoom()
    {
        RoomManager.RoomID = "0003";
        Assert.IsFalse(await RoomManager.JoinRoom(PeerType.Client));
    }


}
