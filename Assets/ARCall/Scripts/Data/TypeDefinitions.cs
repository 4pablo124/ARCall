using System;
using System.Collections.Generic;
using Unity.WebRTC;
using UnityEngine;

public enum PeerType {
    Host,
    Client
}

public struct CountryCode {
    public static string Spain = "+34";
}

public class Message{
    public PeerType peerType;
    public Data data;
}

public class Data{
    public RTCIceCandidateInit ice;
    public RTCSessionDescription sdp;
}

public static class Colors {   
    public class Entry {
        public Entry(string name, string hex){
            this.name=name;
            this.hex=hex;
            Color color;
            ColorUtility.TryParseHtmlString(hex, out color);
            this.color=color;
        }
        public string name;
        public string hex;
        public Color color;
    }

    public static List<Entry> colors = new List<Entry>(){
        new Entry("darkGreen","#57CC99"),
        new Entry("green","#6BDC99"),
        new Entry("lightGreen","#80ED99"),
        new Entry("black","#2E2E2E"),
        new Entry("red","#DC6B6D"),
        new Entry("blue","#6BD4DC"),
        new Entry("yellow","#FFF64A"),
    };

    public static Color GetColorByName(string name) 
    { 
         var entry = colors.Find(c => c.name == name);
         if (entry != null) {
              return entry.color;
         }
         return Color.white;
    }
    public static Color GetColorByHex(string hex) 
    { 
         var entry = colors.Find(c => c.hex == hex);
         if (entry != null) {
              return entry.color;
         }
         return Color.white;
    }
}

// Actualmente no se usa
// public class User{
//     public String Name;
//     public RTCSessionDescription Description;
//     public Dictionary<string,RTCIceCandidateInit> IceCandidates;
// }

// public class Room{
//     public User Host;
//     public User Client;
// }