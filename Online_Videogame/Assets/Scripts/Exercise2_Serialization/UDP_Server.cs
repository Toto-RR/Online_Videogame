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
    public Quaternion rotation;
    public string command; // "MOVE", "SHOOT", "JOIN"
}

public class UDP_Server : MonoBehaviour
{
    private Socket socket;
    private EndPoint clientEndPoint = null;
    private EndPoint remoteEndPoint;
    private bool isServerRunning = false;

    // Datos del propio host
    public string hostName = "HostPlayer";
    public GameObject hostPlayerObject;
    public GameObject clientPlayerPrefab;  // Prefab que representa al cliente en la escena
    private GameObject clientPlayerObject; // GameObject del cliente instanciado

    private ConsoleUI consoleUI;

    private Vector3 lastPos;
    private Quaternion lastRot;

    // Buffer persistente para la recepción de datos
    private byte[] buffer = new byte[1024];

    void Start()
    {
        Application.runInBackground = true;

        if (clientPlayerPrefab == null)
        {
            Debug.LogError("El prefab del cliente no está asignado en el inspector.");
        }

        consoleUI = FindObjectOfType<ConsoleUI>();

        lastPos = hostPlayerObject.transform.position;
        lastRot = hostPlayerObject.transform.rotation;

        StartServer();
    }

    private void Update()
    {
        if (isServerRunning && clientEndPoint != null)
        {
            if (lastRot != hostPlayerObject.transform.rotation || lastPos != hostPlayerObject.transform.position)
            {
                lastRot = hostPlayerObject.transform.rotation;
                lastPos = hostPlayerObject.transform.position;
                BroadcastGameState();
            }
        }
    }

    public void StartServer()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        // Configurar opciones de socket
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 0);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        Debug.Log($"Servidor (Host) iniciado en {((IPEndPoint)socket.LocalEndPoint).Address}:{((IPEndPoint)socket.LocalEndPoint).Port}");
        isServerRunning = true;
        BeginReceive();
    }

    private void BeginReceive()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        Debug.Log("Comenzando a recibir mensajes");
        socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remoteEndPoint, (ar) =>
        {
            try
            {
                int recv = socket.EndReceiveFrom(ar, ref remoteEndPoint);
                string receivedMessage = Encoding.ASCII.GetString(buffer, 0, recv);

                //Registra la dirección del Cliente
                clientEndPoint = remoteEndPoint;

                HandleMessage(receivedMessage);
            }
            catch (SocketException ex)
            {
                Debug.LogError($"Error en la recepción: {ex.Message}");
            }
            finally
            {
                // Continuar escuchando mensajes
                BeginReceive();
            }
        }, null);
    }

    private void HandleMessage(string message)
    {
        PlayerData playerData = JsonUtility.FromJson<PlayerData>(message);
        //consoleUI.LogToConsole($"Mensaje recibido: {message}");

        if (playerData.command == "JOIN")
        {
            consoleUI.LogToConsole($"{playerData.playerName} se ha unido al servidor.");
            if (clientPlayerPrefab != null)
            {
                InstantiateEnemy(playerData);
            }
            BroadcastGameState();
        }
        else if (playerData.command == "MOVE")
        {
            UpdateClientPosition(playerData);
        }
        else if (playerData.command == "SHOOT")
        {
            consoleUI.LogToConsole($"{playerData.playerName} ha disparado.");
            BroadcastShootAction(playerData);
        }

        // Esta linea aqui hace que la posicion del host se actualice siempre que el cliente
        // también se esté moviendo = no me vale
        //BroadcastGameState();
    }

    private void UpdateClientPosition(PlayerData playerData)
    {
        Debug.Log($"El cliente {playerData.playerName} se movió a la posición {playerData.position}");
        if (clientPlayerObject != null)
        {
            clientPlayerObject.transform.position = playerData.position;
            clientPlayerObject.transform.rotation = playerData.rotation;
        }
    }

    public void BroadcastGameState()
    {
        if (clientEndPoint != null && isServerRunning)
        {
            PlayerData hostData = new PlayerData
            {
                playerName = hostName,
                position = hostPlayerObject.transform.position,
                rotation = hostPlayerObject.transform.rotation,
                command = "UPDATE"
            };

            consoleUI.LogToConsole(clientEndPoint.ToString());
            SendData(hostData);
        }
    }

    private void SendData(PlayerData hostData)
    {
        consoleUI.LogToConsole("Sending data");
        string json = JsonUtility.ToJson(hostData);
        byte[] data = Encoding.ASCII.GetBytes(json);
        socket.SendTo(data, data.Length, SocketFlags.None, clientEndPoint);
        consoleUI.LogToConsole("Data send successfull");
    }

    private void BroadcastShootAction(PlayerData shooterData)
    {
        string json = JsonUtility.ToJson(shooterData);
        byte[] data = Encoding.ASCII.GetBytes(json);
        socket.SendTo(data, data.Length, SocketFlags.None, clientEndPoint);
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
        consoleUI.LogToConsole("Instanciando jugador...");
        clientPlayerObject = Instantiate(clientPlayerPrefab, new Vector3(-1, 1, 0), Quaternion.identity);
        clientPlayerObject.name = playerData.playerName;
        clientPlayerObject.SetActive(true);
        Debug.Log("Jugador instanciado por completo");
    }

    void OnApplicationQuit()
    {
        if (socket != null) socket.Close();
    }
}
