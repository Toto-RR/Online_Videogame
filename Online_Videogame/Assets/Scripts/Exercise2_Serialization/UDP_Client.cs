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
    public string PlayerId = Guid.NewGuid().ToString(); // ID único del jugador
    public string PlayerName = "Player";
    public GameObject PlayerObject; // Prefab de jugador

    public GameObject playerPrefab; // Referencia al prefab de jugador
    private Dictionary<string, GameObject> playerObjects = new Dictionary<string, GameObject>(); // Para instanciar jugadores en el cliente

    public GameConfigSO gameConfig;

    void Start()
    {
        Application.runInBackground = true;
        ConnectToServer(gameConfig.PlayerIP, 9050); // Cambia por la IP del servidor
    }

    public void ConnectToServer(string serverIP, int port)
    {
        serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), port);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        SendJoinRequest();
        BeginReceive();
    }

    private void SendJoinRequest()
    {
        PlayerData joinData = new PlayerData
        {
            PlayerId = PlayerId,
            PlayerName = PlayerName,
            Command = "JOIN",
            Position = Vector3.zero,
            Rotation = Quaternion.identity
        };

        string json = JsonUtility.ToJson(joinData);
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

    private void UpdateGameState(GameState gameState)
    {
        foreach (var player in gameState.Players)
        {
            if (!playerObjects.ContainsKey(player.PlayerId))
            {
                // Instanciar el prefab del jugador
                GameObject newPlayer = Instantiate(playerPrefab, player.Position, player.Rotation);
                playerObjects[player.PlayerId] = newPlayer;
                newPlayer.name = player.PlayerName;
            }
            else
            {
                // Actualizar la posición y rotación de los jugadores ya existentes
                GameObject playerObject = playerObjects[player.PlayerId];
                playerObject.transform.position = player.Position;
                playerObject.transform.rotation = player.Rotation;
            }
        }
    }

    void OnApplicationQuit()
    {
        if (socket != null) socket.Close();
    }
}
