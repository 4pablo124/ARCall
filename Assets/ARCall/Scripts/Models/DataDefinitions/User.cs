/// <summary>
/// Representa un usuario en el sistema
/// </summary>
public class User
{
    /// <summary>
    /// Constructor de un usuario
    /// </summary>
    /// <param name="username">Nombre del usuario</param>
    /// <param name="phoneNumber">Teléfono del usuario</param>
    public User(string username = null, string phoneNumber = null)
    {
        this.username = username;
        this.phoneNumber = phoneNumber;
    }

    /// <summary>
    /// Nombre del usuario
    /// </summary>
    public string username;
    /// <summary>
    /// Teléfono del usuario
    /// </summary>
    public string phoneNumber;
}
