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

        // Envía un JOIN al servidor
        SendJoinLobbyRequest();
    }

    private void Start()
    {
        StartCoroutine(WaitForClientInitialization());
    }

    public void SendJoinLobbyRequest()
    {
        PlayerData joinLobbyData = new PlayerData
        {
            Command = CommandType.JOIN_LOBBY,
            PlayerId = PlayerId,
            PlayerName = PlayerName,
            Position = gameConfig.RespawnPos,
            Rotation = gameConfig.RespawnRot,
            Health = player.health.GetCurrentHealth(),
            Energy = player.GetEnergy(),
            AmmoCount = player.GetAmmoCount(),
        };

        playerCommunicator.SendMessage(joinLobbyData);
        Debug.Log("Join request sent");
    }

    public void SendJoinRequest()
    {
        PlayerData joinData = new PlayerData
        {
            Command = CommandType.READY,
            PlayerId = PlayerId,
            PlayerName = PlayerName,
            Position = gameConfig.RespawnPos,
            Rotation = gameConfig.RespawnRot,
            Health = player.health.GetCurrentHealth(),
            Energy = player.GetEnergy(),
            AmmoCount = player.GetAmmoCount(),
        };

        playerCommunicator.SendMessage(joinData);
    }

    public void SendPositionUpdate(Vector3 pos, Quaternion rot)
    {
        PlayerData moveData = new PlayerData
        {
            Command = CommandType.MOVE,
            PlayerId = PlayerId,
            Damage = 0,
            Position = pos,
            Rotation = rot,
        };

        playerCommunicator.SendMessage(moveData);
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

        Debug.Log("Die message sent");
        playerCommunicator.SendMessage(dieData);
    }

    public void HandleRespawn(Vector3 respawnPos, Quaternion respawnRot, float maxHealth)
    {
        PlayerData dieData = new PlayerData
        {
            Command = CommandType.RESPAWN,
            PlayerId = PlayerId,
            Position = respawnPos,
            Rotation = respawnRot,
            Health = maxHealth
        };

        Debug.Log("Respawn message sent");
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