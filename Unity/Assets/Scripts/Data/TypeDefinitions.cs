using System;
using System.Collections.Generic;
using Unity.WebRTC;

public enum PeerType {
    Host,
    Client
}

public class Message{
    public PeerType peerType;
    public Data data;
}

public class Data{
    public RTCIceCandidateInit ice;
    public RTCSessionDescription sdp;
}

// Actualmente no se usa
public class User{
    public String Name;
    public RTCSessionDescription Description;
    public Dictionary<string,RTCIceCandidateInit> IceCandidates;
}

public class Room{
    public User Host;
    public User Client;
}