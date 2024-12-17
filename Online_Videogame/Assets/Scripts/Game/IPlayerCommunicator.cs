public interface IPlayerCommunicator
{
    bool IsConnected { get; }
    void SendMessage(PlayerData message);
    void SendLobbyMessage(LobbyPlayerData message);
}

public class ServerCommunicator : IPlayerCommunicator
{
    public bool IsConnected => true;

    //GAME
    public void SendMessage(PlayerData message)
    {
        UDP_Server.Instance.ServerUpdate(message);
    }

    //LOBBY
    public void SendLobbyMessage(LobbyPlayerData message)
    {
        UDP_Server.Instance.ServerUpdate(message);
    }
}

public class ClientCommunicator : IPlayerCommunicator
{
    public bool IsConnected => UDP_Client.Instance.isConnected;

    //GAME
    public void SendMessage(PlayerData message)
    {
        UDP_Client.Instance.SendMessage(message);
    }

    //LOBBY
    public void SendLobbyMessage(LobbyPlayerData message)
    {
        UDP_Client.Instance.SendMessage(message);
    }
}
