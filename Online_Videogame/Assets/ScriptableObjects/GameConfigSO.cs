using UnityEngine;

public class GameConfigSO : ScriptableObject
{
    [SerializeField] private string playerName;
    [SerializeField] private string playerIP;
    [SerializeField] private string playerRole;
    [SerializeField] private string playerID;

    public string PlayerName { get { return playerName; } }
    public string PlayerIP { get { return playerIP; } }
    public string PlayerRole { get { return playerRole; } }
    public string PlayerID { get { return playerID; } }

    public void Initialize(bool isHost, string name, string ip, string role, string playerid)
    {
        this.playerName = name;
        this.playerIP = ip;
        this.playerRole = role;
        this.playerID = playerid;
    }

    public void SetPlayerName(string name) => playerName = name;
    public void SetPlayerIP(string ip) => playerIP = ip;
    public void SetRole(string role) => playerRole = role;
    public void SetID(string id) => playerID = id;
}
