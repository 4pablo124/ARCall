using Unity.WebRTC;

public class Message
{
    public PeerType peerType;
    public Data data;
}

public class Data
{
    public RTCIceCandidateInit ice;
    public RTCSessionDescription sdp;
}