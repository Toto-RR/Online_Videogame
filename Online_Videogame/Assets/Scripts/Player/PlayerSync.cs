using System;
using UnityEngine;

public class PlayerSync : MonoBehaviour
{
    public string PlayerId;
    public string PlayerName;

    private void Start()
    {
        PlayerId = Guid.NewGuid().ToString();
        PlayerName = "Player_" + UnityEngine.Random.Range(1, 1000);
        SendJoinRequest();
    }

    private void Update()
    {
        SendPositionUpdate();
    }

    private void SendJoinRequest()
    {
        PlayerData joinData = new PlayerData
        {
            Command = "JOIN",
            PlayerId = PlayerId,
            PlayerName = PlayerName,
            Position = transform.position,
            Rotation = transform.rotation
        };

        //UDP_Client.Instance.SendMessage(joinData);
    }

    private void SendPositionUpdate()
    {
        PlayerData moveData = new PlayerData
        {
            Command = "MOVE",
            PlayerId = PlayerId,
            Position = transform.position,
            Rotation = transform.rotation
        };

        //UDP_Client.Instance.SendMessage(moveData);
    }
}
