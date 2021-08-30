using Unity.WebRTC;

/// <summary>
/// Representa un mensaje enviado para realizar una conexión WebRTC
/// </summary>
public class Message
{
    /// <summary>
    /// Rol del usuario que envia el mensaje
    /// </summary>
    public PeerType peerType;
    /// <summary>
    /// Datos a enviar en el mensaje
    /// </summary>
    public Data data;
}

/// <summary>
/// Representa los datos necesarios para poder realizar una conexión WebRTC
/// </summary>
public class Data
{
    /// <summary>
    /// Candidato ICE de la conexión
    /// </summary>
    public RTCIceCandidateInit ice;
    /// <summary>
    /// SDP del contenido a transimitir
    /// </summary>
    public RTCSessionDescription sdp;
}