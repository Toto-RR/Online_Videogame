public interface IPlayerCommunicator
{
    bool IsConnected { get; }
    void SendMessage(PlayerData message);
}

public class ServerCommunicator : IPlayerCommunicator
{
    public bool IsConnected => true;

    public void SendMessage(PlayerData message)
    {
        // Lógica para el servidor para enviar los datos a todos los clientes
        UDP_Server.Instance.ServerUpdate(message);
    }
}

public class ClientCommunicator : IPlayerCommunicator
{
    public bool IsConnected => UDP_Client.Instance.isConnected;

    public void SendMessage(PlayerData message)
    {
        UDP_Client.Instance.SendMessage(message);
    }
}
