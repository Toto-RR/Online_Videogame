using UnityEngine;

public class GameConfigSO : ScriptableObject
{
    public static GameConfigSO Instance;

    [SerializeField] private string playerName;
    [SerializeField] private string playerIP;
    [SerializeField] private string playerRole;
    [SerializeField] private string playerID;
    [SerializeField] private Vector3 respawnPos;
    [SerializeField] private Quaternion respawnRot;

    public string PlayerName { get { return playerName; } }
    public string PlayerIP { get { return playerIP; } }
    public string PlayerRole { get { return playerRole; } }
    public string PlayerID { get { return playerID; } }
    public Vector3 RespawnPos { get { return respawnPos; } }
    public Quaternion RespawnRot { get { return respawnRot; } }

    public void Initialize(
        bool isHost, 
        string name, 
        string ip, 
        string role, 
        string playerid, 
        Vector3 respawnpos, 
        Quaternion respawnrot
        )
    {
        this.playerName = name;
        this.playerIP = ip;
        this.playerRole = role;
        this.playerID = playerid;
        this.respawnPos = respawnpos;
        this.respawnRot = respawnrot;
    }
    private void OnEnable()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void SetPlayerName(string name) => playerName = name;
    public void SetPlayerIP(string ip) => playerIP = ip;
    public void SetRole(string role) => playerRole = role;
    public void SetID(string id) => playerID = id;
    public void SetRespawnPos(Vector3 pos) => respawnPos = pos;
    public void SetRespawnRot(Quaternion rot) => respawnRot = rot;
}
