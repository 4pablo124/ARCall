using System;
using Unity.WebRTC;

public struct User{
    public String Name;
    public RTCSessionDescription Description;
}

public struct Room{
    public User Host;
    public User Client;
}