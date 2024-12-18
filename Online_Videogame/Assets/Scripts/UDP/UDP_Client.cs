using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDP_Client : MonoBehaviour
{
    private Socket socket;
    private EndPoint serverEndPoint;
    internal bool isConnected = false;
    private bool playerInstanciated = false;
    private bool gameStarted = false;

    public GameObject playerPrefab; //Enemy

    internal Dictionary<string, GameObject> playerObjects = new Dictionary<string, GameObject>(); // Para instanciar jugadores en el cliente

    public GameConfigSO gameConfig;

    public static UDP_Client Instance;
    public ConsoleUI consoleUI;

    // Cola para manejar mensajes en el hilo principal
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (!isConnected) // Evita reiniciar si ya está conectado
        {
            ConnectToServer(gameConfig.PlayerIP, 9050);
            consoleUI = FindAnyObjectByType<ConsoleUI>();
        }
        else
        {
            Debug.Log("UDP_Client ya conectado. No se reinicia la conexión.");
        }
    }

    public void ConnectToServer(string serverIP, int port)
    {
        Application.runInBackground = true;
        SocketManager.Instance.InitializeClient(serverIP, port);
        socket = SocketManager.Instance.GetSocket();
        serverEndPoint = SocketManager.Instance.ServerEndPoint;

        isConnected = true;
        consoleUI.LogToConsole("Connected to server");

        BeginReceive();
    }

    // Enviar mensaje al servidor con PlayerData
    public void SendMessage(PlayerData message)
    {
        if (serverEndPoint == null)
        {
            Debug.LogError("Server EndPoint is null.");
            return;
        }

        string json = JsonUtility.ToJson(message);
        byte[] data = Encoding.UTF8.GetBytes(json);
        socket.SendTo(data, data.Length, SocketFlags.None, serverEndPoint);
        //Debug.Log($"PlayerData sent: {json}");
    }

    // Enviar mensaje al servidor con LobbyPlayerData
    public void SendMessage(LobbyPlayerData message)
    {
        if (serverEndPoint == null)
        {
            Debug.LogError("Server EndPoint is null.");
            return;
        }

        string json = JsonUtility.ToJson(message);
        byte[] data = Encoding.UTF8.GetBytes(json);
        socket.SendTo(data, data.Length, SocketFlags.None, serverEndPoint);
        //Debug.Log($"LobbyPlayerData sent: {json}");
    }

    void Update()
    {
        // Procesar mensajes recibidos
        while (messageQueue.TryDequeue(out var message))
        {
            HandleMessage(message);
        }
    }

    private void BeginReceive()
    {
        byte[] buffer = new byte[4096];
        socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref serverEndPoint, (ar) =>
        {
            try
            {
                int recv = socket.EndReceiveFrom(ar, ref serverEndPoint);
                string jsonState = Encoding.UTF8.GetString(buffer, 0, recv);

                // Encola el mensaje para procesarlo en el hilo principal
                messageQueue.Enqueue(jsonState);

                // Continúa escuchando
                BeginReceive();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error en recepción: {ex.Message}");
            }
        }, null);
    }

    private void HandleMessage(string jsonState)
    {
        if (!gameStarted) // LobbyState
        {
            consoleUI.LogToConsole("Receiving lobby state");

            LobbyState lobbyState = JsonUtility.FromJson<LobbyState>(jsonState);

            // Actualiza la UI del lobby
            LobbyManager.Instance?.UpdateLobbyUI(lobbyState.Players);

            // Si el juego ha comenzado, carga la escena del juego
            if (lobbyState.isGameStarted)
            {
                consoleUI.LogToConsole("Game has started. Loading game scene...");
                gameStarted = true;
                SceneLoader.LoadGameScene();
            }
        }
        else
        {
            consoleUI.LogToConsole("Receiving game state");
            GameState gameState = JsonUtility.FromJson<GameState>(jsonState);
            UpdateGameState(gameState);
        }
    }

    private void UpdateGameState(GameState gameState)
    {
        consoleUI.LogToConsole("GameState received");
        if (!playerInstanciated)
        {
            // Asegurarse de que el Player esté inicializado
            if (FindAnyObjectByType<Player>() != null)
            {
                playerInstanciated = true;
            }
            else
            {
                Debug.LogWarning("El objeto Player no está inicializado. Esperando para enviar JoinGameRequest...");
                return;
            }
        }

        HashSet<string> activePlayerIds = new HashSet<string>(playerObjects.Keys);

        foreach (var player in gameState.Players)
        {
            if (player.PlayerId == Player.Instance.playerId)
            {
                UpdateLocalPlayerHealth(player);
                continue;
            }

            if (!playerObjects.ContainsKey(player.PlayerId))
            {
                InstantiatePlayer(player);
            }
            else
            {
                UpdatePlayerPosition(player);
            }

            activePlayerIds.Remove(player.PlayerId);
        }

        RemoveDisconnectedPlayers(activePlayerIds);
    }

    private void UpdateLocalPlayerHealth(PlayerData player)
    {
        float currentHealth = Player.Instance.health.GetCurrentHealth();

        if (player.Health != currentHealth)
        {
            float damage = currentHealth - player.Health;
            if (damage > 0)
            {
                Player.Instance.health.TakeDamage(damage);
            }
            else
            {
                Player.Instance.health.Heal(-damage);
            }
        }
    }

    private void InstantiatePlayer(PlayerData player)
    {
        GameObject newPlayer = Instantiate(playerPrefab, player.Position, player.Rotation);
        PlayerIdentity playerIdentity = newPlayer.GetComponent<PlayerIdentity>();

        if (playerIdentity != null)
        {
            playerIdentity.Initialize(player.PlayerId, player.PlayerName);
        }

        playerObjects[player.PlayerId] = newPlayer;
        newPlayer.name = player.PlayerName;
    }

    private void UpdatePlayerPosition(PlayerData player)
    {
        GameObject playerObject = playerObjects[player.PlayerId];
        playerObject.transform.position = player.Position;
        playerObject.transform.rotation = player.Rotation;
    }

    private void RemoveDisconnectedPlayers(HashSet<string> activePlayerIds)
    {
        foreach (var playerId in activePlayerIds)
        {
            GameObject playerObject = playerObjects[playerId];
            Destroy(playerObject);
            playerObjects.Remove(playerId);
        }
    }

    void OnApplicationQuit()
    {
        PlayerSync.Instance.HandleDisconnect();
        SocketManager.Instance.CloseSocket();
    }
}
