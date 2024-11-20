using System;
using System.Collections;
using UnityEngine;

public class PlayerSync : MonoBehaviour
{
    public static PlayerSync Instance { get; private set; }
    public string PlayerId { get; private set; }
    public string PlayerName { get; private set; }

    private Player player;
    public GameConfigSO gameConfig;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private IPlayerCommunicator playerCommunicator;

    private bool isHost = false;

    private void Awake()
    {
        Instance = this;
        CheckIfHost();

        // Define si es host o cliente
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
        isHost = gameConfig.PlayerRole == "Host";
    }

    private IEnumerator WaitForClientInitialization()
    {
        while (!playerCommunicator.IsConnected)
        {
            Debug.LogWarning("Esperando a que se inicialice la comunicación...");
            yield return null;
        }

        player = Player.Instance;

        PlayerId = Player.Instance.playerId;
        PlayerName = Player.Instance.playerName;

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        // Envía un JOIN al servidor
        SendJoinRequest();
    }

    private void Start()
    {
        StartCoroutine(WaitForClientInitialization());
    }

    public void SendJoinRequest()
    {
        PlayerData joinData = new PlayerData
        {
            Command = CommandType.JOIN,
            PlayerId = PlayerId,
            PlayerName = PlayerName,
            Position = transform.position,
            Rotation = transform.rotation,
            Health = player.health.GetCurrentHealth(),
            Energy = player.GetEnergy(),
            AmmoCount = player.GetAmmoCount(),
        };

        playerCommunicator.SendMessage(joinData);
    }

    public void SendPositionUpdate(Transform transform)
    {
        if (lastPosition != transform.position || lastRotation != transform.rotation)
        {
            lastPosition = transform.position;
            lastRotation = transform.rotation;

            PlayerData moveData = new PlayerData
            {
                Command = CommandType.MOVE,
                PlayerId = PlayerId,
                Position = transform.position,
                Rotation = transform.rotation,
            };

            playerCommunicator.SendMessage(moveData);
        }
    }

    public void HandleShooting(float damage, string targetPlayerId)
    {
        PlayerData shootData = new PlayerData
        {
            Command = CommandType.SHOOT,
            PlayerId = PlayerId,
            TargetPlayerId = targetPlayerId,
            Damage = damage
        };

        playerCommunicator.SendMessage(shootData);
    }

    public void HandleDie()
    {
        PlayerData dieData = new PlayerData
        {
            Command = CommandType.DIE,
            PlayerId = PlayerId,
        };

        playerCommunicator.SendMessage(dieData);
    }

    public void HandleDisconnect()
    {
        PlayerData disconnectData = new PlayerData
        {
            Command = CommandType.DISCONNECTED,
            PlayerId = PlayerId,
        };

        playerCommunicator.SendMessage(disconnectData);
    }
}