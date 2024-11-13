using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string playerName;
    public Vector3 position;
    public string command; // "MOVE", "SHOOT", "JOIN"
}

public class UDP_Server : MonoBehaviour
{
    private Socket socket;
    private EndPoint clientEndPoint;
    private bool isServerRunning = false;

    // Datos del propio host
    public string hostName = "HostPlayer";
    public GameObject hostPlayerObject;
    public GameObject clientPlayerPrefab;  // Prefab que representa al cliente en la escena
    private GameObject clientPlayerObject; // GameObject del cliente instanciado

    private Vector3 hostPosition;

    void Start()
    {
        //GameObject obj = Instantiate(clientPlayerPrefab, new Vector3(0, 3, 0), Quaternion.identity);
        //Debug.Log("Instanciado manualmente en escena");
        Application.runInBackground = true;
        if (clientPlayerPrefab == null)
        {
            Debug.LogError("El prefab del cliente no está asignado en el inspector.");
        }

        StartServer();
    }

    public void StartServer()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        Debug.Log("Servidor (Host) iniciado...");
        isServerRunning = true;
        BeginReceive();
    }

    private void BeginReceive()
    {
        byte[] buffer = new byte[1024];
        clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

        socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref clientEndPoint, (ar) =>
        {
            int recv = socket.EndReceiveFrom(ar, ref clientEndPoint);
            string receivedMessage = Encoding.ASCII.GetString(buffer, 0, recv);

            HandleMessage(receivedMessage);
            BeginReceive();
        }, null);
    }

    private void HandleMessage(string message)
    {
        PlayerData playerData = JsonUtility.FromJson<PlayerData>(message);
        Debug.Log($"Mensaje recibido: {message}");

        if (playerData.command == "JOIN")
        {
            // Confirmación de que el cliente se unió
            Debug.Log($"{playerData.playerName} se ha unido al servidor.");

            // Instanciar el GameObject del cliente en la escena del host
            if (clientPlayerPrefab != null)
            {
                InstantiateEnemy(playerData);
            }

            // Responder con la posición inicial del host
            PlayerData hostData = new PlayerData
            {
                playerName = hostName,
                position = hostPlayerObject.transform.position,
                command = "UPDATE"
            };
            string json = JsonUtility.ToJson(hostData);
            byte[] data = Encoding.ASCII.GetBytes(json);
            socket.SendTo(data, data.Length, SocketFlags.None, clientEndPoint);
        }
        else if (playerData.command == "MOVE")
        {
            // Actualizar posición del cliente
            UpdateClientPosition(playerData);
        }
        else if (playerData.command == "SHOOT")
        {
            // Ejecutar acción de disparo
            Debug.Log($"{playerData.playerName} ha disparado.");
            BroadcastShootAction(playerData);
        }

        // Enviar el estado actualizado del host al cliente
        BroadcastGameState();
    }

    private void UpdateClientPosition(PlayerData playerData)
    {
        Debug.Log($"El cliente {playerData.playerName} se movió a la posición {playerData.position}");
        if (clientPlayerObject != null)
        {
            clientPlayerObject.transform.position = playerData.position;
        }
    }

    private void BroadcastGameState()
    {
        // Enviar la posición del host (este jugador) al cliente
        PlayerData hostData = new PlayerData
        {
            playerName = hostName,
            position = hostPlayerObject.transform.position,
            command = "UPDATE"
        };

        string json = JsonUtility.ToJson(hostData);
        byte[] data = Encoding.ASCII.GetBytes(json);
        socket.SendTo(data, data.Length, SocketFlags.None, clientEndPoint);
    }

    private void BroadcastShootAction(PlayerData shooterData)
    {
        string json = JsonUtility.ToJson(shooterData);
        byte[] data = Encoding.ASCII.GetBytes(json);
        socket.SendTo(data, data.Length, SocketFlags.None, clientEndPoint);
    }

    public void SendHostMovement(Vector3 position)
    {
        hostPosition = position;
        BroadcastGameState();
    }

    public void HostShoot()
    {
        PlayerData hostShootData = new PlayerData
        {
            playerName = hostName,
            position = hostPlayerObject.transform.position,
            command = "SHOOT"
        };
        BroadcastShootAction(hostShootData);
    }

    public void InstantiateEnemy(PlayerData playerData)
    {
        Debug.Log("Instanciando jugador...");
        clientPlayerObject = Instantiate(clientPlayerPrefab, new Vector3(-1, 1, 0), Quaternion.identity);
        
        Debug.Log($"Jugador instanciado: {clientPlayerObject.name}");
        //Instantiate(clientPlayerPrefab, new Vector3(-1, 1, 0), Quaternion.identity, null);
        clientPlayerObject.name = playerData.playerName;

        Debug.Log($"Activo en escena: {clientPlayerObject.activeInHierarchy}");
        clientPlayerObject.SetActive(true);

        Debug.Log("Jugador instanciado por completo");
    }

    void OnApplicationQuit()
    {
        if (socket != null) socket.Close();
    }
}
