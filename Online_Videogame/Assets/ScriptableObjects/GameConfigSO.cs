using UnityEngine;

public class GameConfigSO : ScriptableObject
{
    [SerializeField] private bool isHost;
    [SerializeField] private string playerName;
    [SerializeField] private string playerIP;
    [SerializeField] private string playerRole;

    public bool IsHost { get { return isHost; } }
    public string PlayerName { get { return playerName; } }
    public string PlayerIP { get { return playerIP; } }
    public string PlayerRole { get { return playerRole; } }

    public void Initialize(bool isHost, string name, string ip, string role)
    {
        this.isHost = isHost;
        this.playerName = name;
        this.playerIP = ip;
        this.playerRole = role;
    }

    public void SetPlayerName(string name) => playerName = name;
    public void SetPlayerIP(string ip) => playerIP = ip;
    public void SetRole(string role, bool isHost)
    {
        playerRole = role;
        this.isHost = isHost;
    }
}
