using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDP_Server : MonoBehaviour
{
    private Socket socket;
    private Dictionary<string, EndPoint> connectedClients = new Dictionary<string, EndPoint>();
    private Dictionary<string, GameObject> playerObjects = new Dictionary<string, GameObject>(); // Diccionario para los jugadores instanciados
    private GameState gameState = new GameState();
    private byte[] buffer = new byte[1024];

    public GameObject playerPrefab; // Referencia al prefab del jugador

    void Start()
    {
        Application.runInBackground = true;
        StartServer();
    }

    public void StartServer()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        Debug.Log($"Servidor iniciado en {ipep.Address}:{ipep.Port}");
        BeginReceive();
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

            BroadcastGameState();
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

            // Instanciar el prefab del jugador y agregarlo al diccionario de jugadores
            GameObject playerObject = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            playerObject.name = playerData.PlayerName;
            playerObjects[playerData.PlayerId] = playerObject;

            // Añadir el jugador al estado del juego
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

            // Actualizar el objeto del jugador en la escena
            if (playerObjects.ContainsKey(playerData.PlayerId))
            {
                GameObject playerObject = playerObjects[playerData.PlayerId];
                playerObject.transform.position = playerData.Position;
                playerObject.transform.rotation = playerData.Rotation;
            }
        }
    }

    private void BroadcastGameState()
    {
        string jsonState = JsonUtility.ToJson(gameState);
        byte[] data = Encoding.UTF8.GetBytes(jsonState);

        foreach (var client in connectedClients.Values)
        {
            socket.SendTo(data, data.Length, SocketFlags.None, client);
        }
    }

    void OnApplicationQuit()
    {
        if (socket != null) socket.Close();
    }
}
