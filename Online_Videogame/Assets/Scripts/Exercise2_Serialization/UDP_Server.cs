using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using UnityEngine;

public class UDP_Server : MonoBehaviour
{
    private Socket socket;
    private Dictionary<string, EndPoint> connectedClients = new Dictionary<string, EndPoint>();
    private Dictionary<string, GameObject> playerObjects = new Dictionary<string, GameObject>();
    private GameState gameState = new GameState();
    private byte[] buffer = new byte[1024];

    public GameObject playerPrefab;

    public GameObject serverPlayerObject; // Referencia al objeto del servidor
    private string serverPlayerId;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    void Start()
    {
        Application.runInBackground = true;
        StartServer();
        AddServerAsPlayer();
    }

    private void Update()
    {
        CheckAndSendServerPosition();
    }

    public void StartServer()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        Debug.Log($"Servidor iniciado en {ipep.Address}:{ipep.Port}");
        BeginReceive();
    }

    private void AddServerAsPlayer()
    {
        serverPlayerId = Guid.NewGuid().ToString();
        lastPosition = serverPlayerObject.transform.position;
        lastRotation = serverPlayerObject.transform.rotation;

        PlayerData serverPlayer = new PlayerData
        {
            PlayerId = serverPlayerId,
            PlayerName = "Server",
            Position = lastPosition,
            Rotation = lastRotation,
            Command = "JOIN"
        };

        gameState.Players.Add(serverPlayer);
        playerObjects[serverPlayerId] = serverPlayerObject;
        serverPlayerObject.name = "Server";

        Debug.Log("El jugador del servidor ha sido registrado.");
    }

    private void CheckAndSendServerPosition()
    {
        Vector3 currentPosition = serverPlayerObject.transform.position;
        Quaternion currentRotation = serverPlayerObject.transform.rotation;

        if (currentPosition != lastPosition || currentRotation != lastRotation)
        {
            lastPosition = currentPosition;
            lastRotation = currentRotation;

            UpdateServerPlayerData();
            BroadcastServerPosition();
        }
    }

    private void UpdateServerPlayerData()
    {
        PlayerData serverPlayer = gameState.Players.Find(p => p.PlayerId == serverPlayerId);
        if (serverPlayer != null)
        {
            serverPlayer.Position = lastPosition;
            serverPlayer.Rotation = lastRotation;
        }
    }

    private void BroadcastServerPosition()
    {
        string jsonState = JsonUtility.ToJson(gameState);
        byte[] data = Encoding.UTF8.GetBytes(jsonState);

        foreach (var client in connectedClients.Values)
        {
            socket.SendTo(data, data.Length, SocketFlags.None, client);
        }
    }

    private void BeginReceive()
    {
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remoteEndPoint, (ar) =>
        {
            try
            {
                int receivedBytes = socket.EndReceiveFrom(ar, ref remoteEndPoint);
                string jsonData = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                HandleMessage(jsonData, remoteEndPoint);
                BeginReceive();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error en la recepción: {ex.Message}");
            }
        }, null);
    }

    private void HandleMessage(string jsonData, EndPoint remoteEndPoint)
    {
        try
        {
            PlayerData receivedData = JsonUtility.FromJson<PlayerData>(jsonData);

            switch (receivedData.Command)
            {
                case "JOIN":
                    AddPlayer(receivedData, remoteEndPoint);
                    break;
                case "MOVE":
                    UpdatePlayerPosition(receivedData);
                    break;
                default:
                    Debug.LogWarning($"Comando no reconocido: {receivedData.Command}");
                    break;
            }

            BroadcastServerPosition();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al procesar mensaje: {ex.Message}");
        }
    }

    private void AddPlayer(PlayerData playerData, EndPoint remoteEndPoint)
    {
        if (!connectedClients.ContainsKey(playerData.PlayerId))
        {
            connectedClients[playerData.PlayerId] = remoteEndPoint;

            GameObject playerObject = Instantiate(playerPrefab, playerData.Position, playerData.Rotation);
            playerObject.name = playerData.PlayerName;
            playerObjects[playerData.PlayerId] = playerObject;

            gameState.Players.Add(playerData);

            Debug.Log($"Jugador {playerData.PlayerName} añadido.");
        }
    }

    private void UpdatePlayerPosition(PlayerData playerData)
    {
        PlayerData player = gameState.Players.Find(p => p.PlayerId == playerData.PlayerId);
        if (player != null)
        {
            player.Position = playerData.Position;
            player.Rotation = playerData.Rotation;

            if (playerObjects.ContainsKey(playerData.PlayerId))
            {
                GameObject playerObject = playerObjects[playerData.PlayerId];
                playerObject.transform.position = playerData.Position;
                playerObject.transform.rotation = playerData.Rotation;
            }
        }
    }

    void OnApplicationQuit()
    {
        if (socket != null) socket.Close();
    }
}
