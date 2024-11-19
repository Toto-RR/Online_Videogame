using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class UDP_Client : MonoBehaviour
{
    private Socket socket;
    private EndPoint serverEndPoint;
    internal bool isConnected = false;

    public GameObject playerPrefab; //Enemy

    private Dictionary<string, GameObject> playerObjects = new Dictionary<string, GameObject>(); // Para instanciar jugadores en el cliente

    public GameConfigSO gameConfig;

    public static UDP_Client Instance;
    public ConsoleUI consoleUI;

    private int lastPlayerCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Application.runInBackground = true;
        ConnectToServer(gameConfig.PlayerIP, 9050);
        consoleUI = FindAnyObjectByType<ConsoleUI>();

    }

    public void ConnectToServer(string serverIP, int port)
    {
        serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), port);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        isConnected = true;

        BeginReceive();
    }

    // Send message to the server
    public void SendMessage(BaseMessage message)
    {
        if (serverEndPoint == null)
        {
            Debug.LogError("Server EndPoint is null.");
            return;
        }

        string json = JsonUtility.ToJson(message);
        byte[] data = Encoding.UTF8.GetBytes(json);
        socket.SendTo(data, data.Length, SocketFlags.None, serverEndPoint);
    }

    private void BeginReceive()
    {
        byte[] buffer = new byte[1024];
        socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref serverEndPoint, (ar) =>
        {
            try
            {
                int recv = socket.EndReceiveFrom(ar, ref serverEndPoint);
                string jsonState = Encoding.UTF8.GetString(buffer, 0, recv);

                // The client always receive a game state, so it only has to update the list of player data
                GameState gameState = JsonUtility.FromJson<GameState>(jsonState);
                UpdateGameState(gameState);

                BeginReceive();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error en recepción: {ex.Message}");
            }
        }, null);
    }

    // Update the game state updating the player data of each player on the list
    private void UpdateGameState(GameState gameState)
    {
        foreach (var player in gameState.Players)
        { 
            if (player.PlayerId == PlayerSync.Instance.PlayerId)
            {
                //It does not update its own status because it has already been updated manually (player we control)
                continue;
            }
            if (!playerObjects.ContainsKey(player.PlayerId))
            {
                // If is a new player instantiate the player
                GameObject newPlayer = Instantiate(playerPrefab, player.Position, player.Rotation);
                playerObjects[player.PlayerId] = newPlayer;
                newPlayer.name = player.PlayerName;
            }
            else
            {
                // Update the position and rotation
                GameObject playerObject = playerObjects[player.PlayerId];
                playerObject.transform.position = player.Position;
                playerObject.transform.rotation = player.Rotation;
            }
        }

        consoleUI.LogToConsole("Jugadores conectados: " + gameState.Players.Count);
    }

    void OnApplicationQuit()
    {
        if (socket != null) socket.Close();
    }
}
