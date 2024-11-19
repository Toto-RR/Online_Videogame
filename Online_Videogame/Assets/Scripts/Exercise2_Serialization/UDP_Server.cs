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
    public Dictionary<string, GameObject> playerObjects = new Dictionary<string, GameObject>();
    private GameState gameState = new GameState();
    private byte[] buffer = new byte[1024];
    public static UDP_Server Instance;

    public GameObject playerPrefab;
    public ConsoleUI consoleUI;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        consoleUI = FindAnyObjectByType<ConsoleUI>();

        Application.runInBackground = true;
        StartServer();
    }

    public void StartServer()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        Debug.Log($"Server started at: {ipep.Address}:{ipep.Port}");
        BeginReceive();
    }

    public void StopServer()
    {
        try
        {
            socket.Close();
            socket = null;

            Debug.Log("Servidor detenido.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al detener el servidor: {ex.Message}");
        }
    }

    public void StartManually()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Iniciando Server manualmente");

            // Verificar si el socket ya está abierto
            if (socket != null)
            {
                try
                {
                    Debug.Log("Cerrando socket existente...");
                    StopServer();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error al cerrar el socket: {ex.Message}");
                }
            }

            // Reiniciar el servidor
            StartServer();
        }
    }

    public void Update()
    {
        StartManually();
    }

    // Add and update the server player game object
    public void ServerUpdate(PlayerData playerData)
    {
        ProcessMessage(playerData);
        BroadcastGameState();
    }

    // Broadcast the game state (all players data) to all players
    private void BroadcastGameState()
    {
        string jsonState = JsonUtility.ToJson(gameState);
        byte[] data = Encoding.UTF8.GetBytes(jsonState);

        foreach (var client in connectedClients.Values)
        {
            socket.SendTo(data, data.Length, SocketFlags.None, client);
        }
    }

    // Start to receive messages, recursive
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
                Debug.LogError($"Error: {ex.Message}");
            }
        }, null);
    }

    // Process the message and broadcast to all clients
    private void HandleMessage(string jsonData, EndPoint remoteEndPoint)
    {
        try
        {
            // To answer client when ask if server is started
            if (jsonData == "ping")
            {
                HandlePing(remoteEndPoint);
                return;
            }

            //consoleUI.LogToConsole("Received: " + jsonData);
            PlayerData receivedData = JsonUtility.FromJson<PlayerData>(jsonData);

            ProcessMessage(receivedData, remoteEndPoint);
            BroadcastGameState();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error processing message: {ex.Message}");
        }
    }

    // Process the received message
    private void ProcessMessage(PlayerData receivedData, EndPoint remoteEndPoint = null)
    {
        switch (receivedData.Command)
        {
            case "JOIN":
                consoleUI.LogToConsole("Player joined " + receivedData.PlayerName);
                AddPlayer(receivedData, remoteEndPoint);
                break;
            case "MOVE":
                UpdatePlayerPosition(receivedData);
                break;
            case "SHOOT":
                ProcessShoot(receivedData);
                break;
            case "DIE":
                consoleUI.LogToConsole($"{receivedData.PlayerId} HA MUERTO");
                break;
            default:
                Debug.LogWarning($"Unknown command: {receivedData.Command}");
                break;
        }
    }

    // Add the player to the scene if his not the server player (he is already on the scene)
    private void AddPlayer(PlayerData playerData, EndPoint remoteEndPoint = null)
    {
        //If is the player that we control we dont need to instanciate a prefab because we already have the player on the scene
        //TODO: Instanciate the player that we control right here so it can be deleted from the scene (better(?))
        if (playerData.PlayerId == PlayerSync.Instance.PlayerId)
        {
            AddPlayerToList(playerData);
            return;
        }

        if (!connectedClients.ContainsKey(playerData.PlayerId))
        {
            connectedClients[playerData.PlayerId] = remoteEndPoint;

            GameObject playerObject = Instantiate(playerPrefab, playerData.Position, playerData.Rotation);

            PlayerIdentity playerIdentity = playerObject.GetComponent<PlayerIdentity>();
            if (playerIdentity != null)
            {
                playerIdentity.Initialize(playerData.PlayerId, playerData.PlayerName);
            }
            //else Debug.Log("Prefab doesn't have PlayerIdentity");

            playerObject.name = playerData.PlayerName;
            playerObjects[playerData.PlayerId] = playerObject;

            AddPlayerToList(playerData);
        }
    }

    // Update all the position/rotation players data (LOCALLY)
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

    private void ProcessShoot(PlayerData shooterData)
    {
        // Encuentra los datos del jugador objetivo en el estado del juego
        PlayerData targetPlayerData = gameState.Players.Find(p => p.PlayerId == shooterData.TargetPlayerId);
        string shooterName = GetPlayerName(shooterData);
        string targetName = GetPlayerName(targetPlayerData);

        if (targetPlayerData != null)
        {
            //consoleUI.LogToConsole($"{shooterName} shoot to {targetName}");

            targetPlayerData.Health -= shooterData.Damage;
            //consoleUI.LogToConsole($"{targetName} has {targetPlayerData.Health}");

            // Target is the player that we controll
            if (targetPlayerData.PlayerId == Player.Instance.playerId)
            {
                //Debug.Log($"{targetName} (local player) was hit. Applying damage locally.");

                Player localPlayer = FindObjectOfType<Player>();
                if (localPlayer != null)
                {
                    localPlayer.TakeDamage(shooterData.Damage);
                }
                else
                {
                    Debug.LogWarning("Local Player script not found!");
                }
            }

            // Actualiza el estado y retransmite a todos los jugadores
            BroadcastGameState();
        }
        else
        {
            Debug.LogWarning($"Jugador objetivo {shooterData.TargetPlayerId} no encontrado en el estado del juego.");
        }
    }


    // Add the player to the gamestate list
    void AddPlayerToList(PlayerData playerData)
    {
        gameState.Players.Add(playerData);
        Debug.Log("Name: " + playerData.PlayerName);
        Debug.Log("Players: " + gameState.Players.Count);
    }

    private string GetPlayerName(PlayerData playerData)
    {
        PlayerData data = gameState.Players.Find(p => p.PlayerId == playerData.PlayerId);
        return data.PlayerName;
    }

    private void HandlePing(EndPoint clientEndPoint)
    {
        try
        {
            string pongMessage = "pong";
            byte[] pongData = Encoding.UTF8.GetBytes(pongMessage);

            // Enviar respuesta "pong" al cliente
            socket.SendTo(pongData, clientEndPoint);
            Debug.Log($"Responded to ping from {clientEndPoint}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error responding to ping: {ex.Message}");
        }
    }

    void OnApplicationQuit()
    {
        StopServer();
    }
}
