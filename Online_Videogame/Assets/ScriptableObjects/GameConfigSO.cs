using UnityEngine;

public class GameConfigSO : ScriptableObject
{
    [SerializeField] private string playerName;
    [SerializeField] private string playerIP;
    [SerializeField] private string playerRole;

    public string PlayerName { get { return playerName; } }
    public string PlayerIP { get { return playerIP; } }
    public string PlayerRole { get { return playerRole; } }

    public void Initialize(bool isHost, string name, string ip, string role)
    {
        this.playerName = name;
        this.playerIP = ip;
        this.playerRole = role;
    }

    public void SetPlayerName(string name) => playerName = name;
    public void SetPlayerIP(string ip) => playerIP = ip;
    public void SetRole(string role) => playerRole = role;
}
