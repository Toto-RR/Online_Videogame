using System;
using System.Collections;
using UnityEngine;

public class PlayerSync : MonoBehaviour
{
    public static PlayerSync Instance { get; private set; }
    public string PlayerId { get; private set; } 
    public string PlayerName { get; private set; }

    public GameConfigSO gameConfig;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private void Awake()
    {
        Instance = this;
    }

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
