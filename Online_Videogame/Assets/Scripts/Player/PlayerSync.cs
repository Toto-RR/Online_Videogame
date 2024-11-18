using System;
using System.Collections;
using UnityEngine;

public class PlayerSync : MonoBehaviour
{
    private string PlayerId;
    private string PlayerName;

    public GameConfigSO gameConfig;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private IEnumerator WaitForClientInitialization()
    {
        while (UDP_Client.Instance.isConnected == false)
        {
            Debug.LogWarning("Esperando a que UDP_Client se inicialice...");
            yield return null;
        }

        PlayerId = Guid.NewGuid().ToString();
        PlayerName = string.IsNullOrEmpty(gameConfig.PlayerName) ? "Player1" : gameConfig.PlayerName;
        Debug.Log("Player name: " + PlayerName);

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        SendJoinRequest();
    }

    private void Start()
    {
        StartCoroutine(WaitForClientInitialization());
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

        Debug.Log("Join request sent!");
        Debug.Log("Position sent: " + transform.position);
        UDP_Client.Instance.SendMessage(joinData);
    }

    private void SendPositionUpdate()
    {
        if (lastPosition != transform.position || lastRotation != transform.rotation)
        {
            lastPosition = transform.position;
            lastRotation = transform.rotation;

            PlayerData moveData = new PlayerData
            {
                Command = "MOVE",
                PlayerId = PlayerId,
                Position = transform.position,
                Rotation = transform.rotation
            };

            UDP_Client.Instance.SendMessage(moveData);
        }
    }
}
