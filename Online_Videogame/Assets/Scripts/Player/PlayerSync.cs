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

        // Definir qué tipo de comunicador se va a usar, si es servidor o cliente.
        if (isHost)
        {
            playerCommunicator = new ServerCommunicator();  // Usa la clase del servidor
        }
        else
        {
            playerCommunicator = new ClientCommunicator();  // Usa la clase del cliente
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

        PlayerId = Guid.NewGuid().ToString();
        PlayerName = string.IsNullOrEmpty(gameConfig.PlayerName) ? "Player1" : gameConfig.PlayerName;
        Debug.Log("Player name: " + PlayerName);

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        // Enviar solicitud de "JOIN" o cualquier otro comando inicial
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