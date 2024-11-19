using UnityEngine;

public class PlayerIdentity : MonoBehaviour
{
    public string PlayerId;
    public string PlayerName;

    public void Initialize(string playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
    }
}
