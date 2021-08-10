using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UIElements;

public class AudioManager : MonoBehaviour
{
    public event Action<byte[], int> OnEncoded;
    [HideInInspector] public bool audioConected = false;
    private MyEncoder encoder;
    private MyDecoder decoder;

    
    private byte[] audioBuffer;

    void Awake()
    {
        encoder = GameObject.Find("AudioInput").GetComponent<MyEncoder>();
        decoder = GameObject.Find("AudioOutput").GetComponent<MyDecoder>();

    }

    private void Start() {
        encoder.OnEncoded += (data, length) => OnEncoded?.Invoke(data, length);
        
    }

    public byte[] Encode(byte[] data, int length) {
        return data != null ? EncodeLength(data, length) : new byte[1];
    }
    
    public void Decode(byte[] bytes){
        if(bytes.Length != 1){
            int size = sizeof(int);
            byte[] encodedLengthBytes = bytes.Take(size).ToArray();
            byte[] encodedAudioBytes = bytes.Skip(sizeof(int)).Take(bytes.Length - sizeof(int)).ToArray();
            int length = DecodeLength(encodedLengthBytes);
            
            decoder.Decode(encodedAudioBytes, length);
        }else{
            decoder.Decode(null, 0);
        }
    }
    
    private byte[] EncodeLength(byte[] bytes, int length){
        int[] lengthArr = new int[] { length };
        byte[] result = new byte[lengthArr.Length * sizeof(int)];
        Buffer.BlockCopy(lengthArr, 0, result, 0, result.Length);
        return AddByteToArray(bytes, result);
    }
    private int DecodeLength(byte[] bytes){
        int result = BitConverter.ToInt32(bytes, 0);
        return result;
    }
    private byte[] AddByteToArray(byte[] bArray, byte[] newBytes){
        byte[] newArray = new byte[bArray.Length + newBytes.Length];
        bArray.CopyTo(newArray, newBytes.Length);
        newBytes.CopyTo(newArray, 0);
        return newArray;
    }
}
