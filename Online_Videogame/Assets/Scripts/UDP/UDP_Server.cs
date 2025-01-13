using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using UnityEngine;
using Unity.VisualScripting;
using System.Security;
using System.Collections.Concurrent;

public class UDP_Server : MonoBehaviour
{
    private Socket socket;
    private Dictionary<string, EndPoint> connectedClients = new Dictionary<string, EndPoint>();
    public Dictionary<string, GameObject> playerObjects = new Dictionary<string, GameObject>();
    public GameState gameState = new GameState();
    private byte[] buffer = new byte[1024];
    public static UDP_Server Instance;

    public LobbyManager lobbyManager;
    public LobbyState lobbyState = new LobbyState();
    private bool inGame = false; // Estado: lobby o juego

    public GameObject playerPrefab;
    public ConsoleUI consoleUI;

    private ConcurrentQueue<(string, EndPoint)> messageQueue = new ConcurrentQueue<(string, EndPoint)>();

    // --- Jitter mitigation ---
    public bool jitter = true;
    public bool packetLoss = true;
    public int minJitt = 0;
    public int maxJitt = 800;
    public int lossThreshold = 90;

    private List<Message> messageBuffer = new List<Message>();
    private object myLock = new object();
    private bool exit = false;

    public struct Message
    {
        public byte[] message;
        public DateTime time;
        public uint id;
        public IPEndPoint ip;
    }

    // --- --- --- --- --- --- ---

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        consoleUI = FindAnyObjectByType<ConsoleUI>();

        StartServer();
    }

    public void StartServer()
    {
        if (socket != null && socket.IsBound)
        {
            Debug.LogWarning("El servidor ya está en ejecución.");
            return;
        }

        Application.runInBackground = true;
        SocketManager.Instance.InitializeServer(9050); // Inicia el socket del servidor
        socket = SocketManager.Instance.GetSocket();

        Debug.Log("Servidor iniciado.");
        BeginReceive();
    }


    public void StopServer()
    {
        try
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;

                Debug.Log("Servidor detenido.");
            }
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

        // Procesar mensajes en el hilo principal
        while (messageQueue.TryDequeue(out var message))
        {
            HandleMessage(message.Item1, message.Item2);
        }
    }

    // Add and update the server player game object
    public void ServerUpdate(PlayerData playerData)
    {
        if (inGame)
        {
            ProcessMessage(playerData);
            BroadcastGameState();
        }
    }

    public void ServerUpdate(LobbyPlayerData playerData)
    {
        if (!inGame)
        {
            ProcessLobbyMessage(playerData);
            BroadcastLobbyState();
        }
    }
    public void sendMessages(Socket socket)
    {
        Debug.Log("Iniciando envío de mensajes con jitter...");
        while (!exit)
        {
            DateTime now = DateTime.Now;

            lock (myLock)
            {
                for (int i = messageBuffer.Count - 1; i >= 0; i--)
                {
                    Message m = messageBuffer[i];
                    if (m.time <= now)
                    {
                        socket.SendTo(m.message, m.message.Length, SocketFlags.None, m.ip);
                        messageBuffer.RemoveAt(i);
                        Debug.Log($"Mensaje enviado a {m.ip}");
                    }
                }
            }
        }
    }

    public void sendMessage(byte[] text, IPEndPoint ip)
    {
        System.Random r = new System.Random();
        if (((r.Next(0, 100) > lossThreshold) && packetLoss) || !packetLoss)
        {
            Message m = new Message
            {
                message = text,
                time = jitter ? DateTime.Now.AddMilliseconds(r.Next(minJitt, maxJitt)) : DateTime.Now,
                id = 0,
                ip = ip
            };

            lock (myLock)
            {
                messageBuffer.Add(m);
            }
            Debug.Log($"Mensaje programado para {m.time}");
        }
    }

    private void BroadcastLobbyState()
    {
        var uniquePlayers = new HashSet<string>();
        lobbyState.Players.RemoveAll(p => !uniquePlayers.Add(p.PlayerId));

        string jsonState = JsonUtility.ToJson(lobbyState);
        //Debug.Log($"Tamaño del mensaje: {Encoding.UTF8.GetBytes(jsonState).Length} bytes");
        byte[] data = Encoding.UTF8.GetBytes(jsonState);

        foreach (var client in connectedClients.Values)
        {
            socket.SendTo(data, data.Length, SocketFlags.None, client);
        }
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
        if (socket == null || !socket.IsBound)
        {
            Debug.LogWarning("El socket está cerrado o no está enlazado. Deteniendo recepción.");
            return;
        }

        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        try
        {
            socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remoteEndPoint, (ar) =>
            {
                try
                {
                    if (socket == null || !socket.IsBound)
                    {
                        Debug.LogWarning("El socket está cerrado antes de finalizar la recepción.");
                        return;
                    }

                    int receivedBytes = socket.EndReceiveFrom(ar, ref remoteEndPoint);
                    string jsonData = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                    // Encolar el mensaje para procesarlo en el hilo principal
                    messageQueue.Enqueue((jsonData, remoteEndPoint));

                    // Continuar recibiendo
                    BeginReceive();
                }
                catch (ObjectDisposedException)
                {
                    Debug.LogWarning("El socket fue cerrado mientras se esperaba recepción.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error en recepción: {ex.Message}");
                }
            }, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al iniciar recepción: {ex.Message}");
        }
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

            if (!inGame)
            {
                LobbyPlayerData lobbyPlayer = JsonUtility.FromJson<LobbyPlayerData>(jsonData);
                ProcessLobbyMessage(lobbyPlayer, remoteEndPoint);
                BroadcastLobbyState();
            }
            else
            {
                //consoleUI.LogToConsole("Received: " + jsonData);
                PlayerData receivedData = JsonUtility.FromJson<PlayerData>(jsonData);
                ProcessMessage(receivedData, remoteEndPoint);
                BroadcastGameState();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error processing message: {ex.Message}");
        }
    }

    private void ProcessLobbyMessage(LobbyPlayerData receivedData, EndPoint remoteEndPoint = null)
    {
        switch (receivedData.Command)
        {
            case LobbyCommandType.JOIN_LOBBY:
                AddPlayerToLobby(receivedData, remoteEndPoint);
                break;
            case LobbyCommandType.READY:
                lobbyState.SetPlayerReady(receivedData.PlayerId, receivedData.PlayerColor);
                break;
            case LobbyCommandType.START_GAME:
                if (lobbyState.AreAllPlayersReady())
                {
                    //Debug.Log("All players are ready. Starting the game...");
                    lobbyState.isGameStarted = true; // Actualiza el estado
                    BroadcastLobbyState(); // Envia LobbyState actualizado
                    inGame = true; // Cambia el estado interno del servidor
                    SceneLoader.LoadGameScene();
                }
                else
                {
                    Debug.LogWarning("Cannot start game. Not all players are ready.");
                }
                break;

            default:
                Debug.LogWarning($"Unknown command: {receivedData.Command}");
                break;
        }
    }

    // Process the received message
    private void ProcessMessage(PlayerData receivedData, EndPoint remoteEndPoint = null)
    {
        switch (receivedData.Command)
        {
            case CommandType.JOIN_GAME:
                //consoleUI.LogToConsole("Player joined " + receivedData.PlayerName);
                var lobbyPlayer = lobbyState.Players.Find(p => p.PlayerId == receivedData.PlayerId);
                if (lobbyPlayer != null)
                {
                    receivedData.PlayerColor = lobbyPlayer.PlayerColor;
                    Debug.Log($"Color transferido desde LobbyState: {receivedData.PlayerColor}");
                }
                AddPlayer(receivedData, remoteEndPoint);
                break;
            case CommandType.MOVE:
                UpdatePlayerPosition(receivedData);
                break;
            case CommandType.SHOOT:
                ProcessShoot(receivedData);
                break;
            case CommandType.DIE:
                //consoleUI.LogToConsole($"{receivedData.PlayerId} HA MUERTO");
                HandleDie(receivedData);
                break;
            case CommandType.RESPAWN:
                //consoleUI.LogToConsole($"{receivedData.PlayerId} RESPAWN");
                HandleRespawn(receivedData);
                break;
            case CommandType.DISCONNECTED:
                //consoleUI.LogToConsole($"{receivedData.PlayerId} DISCONNECTED");
                HandleDisconnect(receivedData);
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
            Debug.Log("Añadiendo Host a gamestate");
            AddPlayerToList(playerData);
            return;
        }

        if (connectedClients.ContainsKey(playerData.PlayerId) && !gameState.Players.Exists(p => p.PlayerId == playerData.PlayerId))
        {
            Debug.Log("Añadiendo nuevo cliente...");
            GameObject playerObject = Instantiate(playerPrefab, playerData.Position, playerData.Rotation);
            Renderer playerRenderer = playerObject.GetComponentInChildren<Renderer>();
            if (playerRenderer != null)
            {
                playerRenderer.material.color = playerData.PlayerColor;
                Debug.Log($"Color aplicado al jugador {playerData.PlayerName}: {playerData.PlayerColor}");
            }

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

        if (targetPlayerData != null)
        {
            targetPlayerData.Health -= shooterData.Damage;

            // Target is the player that we controll
            if (targetPlayerData.PlayerId == Player.Instance.playerId)
            {
                Player.Instance.TakeDamage(shooterData.Damage);
            }

            // Actualiza el estado y retransmite a todos los jugadores
            //BroadcastGameState();
        }
        else
        {
            Debug.LogWarning($"Jugador objetivo {shooterData.TargetPlayerId} no encontrado en el estado del juego.");
        }
    }

    private void HandleDie(PlayerData player)
    {
        try
        {
            Debug.Log($"Jugador {player.PlayerId} ha muerto y respawneado.");
            //consoleUI.LogToConsole($"Jugador {player.PlayerId} ha muerto y respawneado.");

            // Código de la función
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error en HandleDie: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void HandleRespawn(PlayerData playerData)
    {
        PlayerData player = gameState.Players.Find(p => p.PlayerId == playerData.PlayerId);
        if (player != null)
        {
            player.Position = playerData.Position;
            player.Rotation = playerData.Rotation;
            player.Health = playerData.Health;

            if (playerObjects.ContainsKey(playerData.PlayerId))
            {
                GameObject playerObject = playerObjects[playerData.PlayerId];
                playerObject.transform.position = playerData.Position;
                playerObject.transform.rotation = playerData.Rotation;
            }

            // Actualiza el estado del servidor y lo transmite
            //BroadcastGameState();
        }
    }


    private void HandleDisconnect(PlayerData playerData)
    {
        if (connectedClients.ContainsKey(playerData.PlayerId))
        {
            connectedClients.Remove(playerData.PlayerId);

            // Eliminar el objeto del jugador en el servidor
            if (playerObjects.ContainsKey(playerData.PlayerId))
            {
                GameObject playerObject = playerObjects[playerData.PlayerId];
                Destroy(playerObject);  // Elimina el jugador de la escena
                playerObjects.Remove(playerData.PlayerId);
            }

            // Actualiza el estado del juego
            gameState.Players.RemoveAll(p => p.PlayerId == playerData.PlayerId);
            lobbyState.Players.RemoveAll(p => p.PlayerId == playerData.PlayerId);

            // Notificar a todos los clientes sobre la desconexión
            //BroadcastGameState();

            Debug.Log($"Jugador {playerData.PlayerId} se ha desconectado.");
        }

    }

    // Add the player to the gamestate list
    void AddPlayerToList(PlayerData playerData)
    {
        gameState.Players.Add(playerData);
        Debug.Log("Player added Name: " + playerData.PlayerName);
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
            //Debug.Log($"Responded to ping from {clientEndPoint}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error responding to ping: {ex.Message}");
        }
    }

    private void AddPlayerToLobby(LobbyPlayerData playerData, EndPoint remoteEndPoint = null)
    {
        if (playerData.PlayerId == PlayerSync.Instance.PlayerId)
        {
            lobbyState.AddPlayer(playerData);
            Debug.Log("Player Added");
        }
        else
        {
            if (!connectedClients.ContainsKey(playerData.PlayerId))
            {
                connectedClients[playerData.PlayerId] = remoteEndPoint;
                lobbyState.AddPlayer(playerData);
                Debug.Log($"Player {playerData.PlayerName} added to LobbyState.");
            }
        }
        LobbyManager.Instance.UpdateLobbyUI(lobbyState.Players);
    }


    void OnApplicationQuit()
    {
        try
        {
            if (socket != null)
            {
                socket.Close();
            }

            PlayerSync.Instance?.HandleDisconnect();
            SocketManager.Instance?.CloseSocket();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al cerrar el socket en OnApplicationQuit: {ex.Message}");
        }
    }


}
