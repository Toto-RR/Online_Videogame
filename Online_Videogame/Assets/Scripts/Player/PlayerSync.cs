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

    private IPlayerCommunicator playerCommunicator;

    private bool isHost = false;

    private void Awake()
    {
        Instance = this;
        CheckIfHost();

        // Define if the player is gonna use the server or the client communicator (is host or is client)
        if (isHost)
        {
            playerCommunicator = new ServerCommunicator();
        }
        else
        {
            playerCommunicator = new ClientCommunicator();
        }
    }

    private void CheckIfHost()
    {
        if (gameConfig.PlayerRole == "Host") isHost = true;
        else isHost = false;
    }

    private IEnumerator WaitForClientInitialization()
    {
        while (!playerCommunicator.IsConnected)
        {
            Debug.LogWarning("Esperando a que se inicialice la comunicación...");
            yield return null;
        }

        //Unique ID
        PlayerId = Guid.NewGuid().ToString();

        //Player name
        PlayerName = string.IsNullOrEmpty(gameConfig.PlayerName) ? "Player1" : gameConfig.PlayerName;

        //Save the last position to check if the player moves to new one (that is the trigger to send data info)
        lastPosition = transform.position;
        lastRotation = transform.rotation;

        // Send the Join Request at start
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

        playerCommunicator.SendMessage(joinData);
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

            playerCommunicator.SendMessage(moveData);
        }
    }
}