using System;
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

    public GameObject playerPrefab; //Enemy

    internal Dictionary<string, GameObject> playerObjects = new Dictionary<string, GameObject>(); // Para instanciar jugadores en el cliente

    public GameConfigSO gameConfig;

    public static UDP_Client Instance;
    public ConsoleUI consoleUI;

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
        //Debug.Log($"{json}");
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
                Debug.LogError($"Error en recepci�n: {ex.Message}");
            }
        }, null);
    }

    // Update the game state updating the player data of each player on the list
    private void UpdateGameState(GameState gameState)
    {
        // Paso 1: Crear un conjunto de IDs de jugadores actualmente conectados en el cliente
        HashSet<string> activePlayerIds = new HashSet<string>(playerObjects.Keys);

        // Paso 2: Iterar sobre los jugadores recibidos en el GameState
        foreach (var player in gameState.Players)
        {
            // Verifica si el jugador es local
            if (player.PlayerId == Player.Instance.playerId)
            {
                UpdateLocalPlayerHealth(player);
                continue;
            }

            // Si el jugador no est� en el cliente, instanciarlo
            if (!playerObjects.ContainsKey(player.PlayerId))
            {
                InstantiatePlayer(player);
            }
            else
            {
                // Si ya est� instanciado, actualizar su posici�n y rotaci�n
                UpdatePlayerPosition(player);
            }

            // Eliminarlo del conjunto de jugadores activos ya procesados
            activePlayerIds.Remove(player.PlayerId);
        }

        // Paso 3: Eliminar jugadores que ya no est�n en el GameState (desconectados)
        RemoveDisconnectedPlayers(activePlayerIds);
    }

    // Actualiza la salud del jugador local
    private void UpdateLocalPlayerHealth(PlayerData player)
    {
        float currentHealth = Player.Instance.health.GetCurrentHealth();

        // Si la salud es diferente, calcula el daño recibido
        if (player.Health != currentHealth)
        {
            float damage = currentHealth - player.Health;

            // Si el daño es positivo, significa que se recibió daño
            if (damage > 0)
            {
                Player.Instance.health.TakeDamage(damage);
            }
            else
            {
                // Si el daño es negativo, significa que se curó
                Player.Instance.health.Heal(-damage);
            }
        }
    }

    // Instancia un nuevo jugador
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

    // Actualiza la posici�n y rotaci�n de un jugador ya instanciado
    private void UpdatePlayerPosition(PlayerData player)
    {
        GameObject playerObject = playerObjects[player.PlayerId];
        playerObject.transform.position = player.Position;
        playerObject.transform.rotation = player.Rotation;
    }

    // Elimina a los jugadores desconectados de la escena
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
        if (socket != null) socket.Close();
    }
}