using System;
using System.Collections.Generic;
using Unity.WebRTC;

public class User{
    public String Name;
    public RTCSessionDescription Description;
    public List<RTCIceCandidateInit> IceCandidates;
}

public class Room{
    public User Host;
    public User Client;
}