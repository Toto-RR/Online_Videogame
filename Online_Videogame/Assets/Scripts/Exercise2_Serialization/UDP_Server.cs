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
                AddPlayer(receivedData, remoteEndPoint);
                break;
            case "MOVE":
                UpdatePlayerPosition(receivedData);
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
        }

        if (!connectedClients.ContainsKey(playerData.PlayerId))
        {
            connectedClients[playerData.PlayerId] = remoteEndPoint;

            GameObject playerObject = Instantiate(playerPrefab, playerData.Position, playerData.Rotation);
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

    // Add the player to the gamestate list
    void AddPlayerToList(PlayerData playerData)
    {
        gameState.Players.Add(playerData);
        consoleUI.LogToConsole("Player added: " + playerData.PlayerName);
        consoleUI.LogToConsole("Total Players: " + gameState.Players.Count);
    }

    void OnApplicationQuit()
    {
        if (socket != null) socket.Close();
    }
}
