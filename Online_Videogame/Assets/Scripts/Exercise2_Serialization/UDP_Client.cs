using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDP_Client : MonoBehaviour
{
    private Socket socket;
    private EndPoint serverEndPoint;
    public string playerName = "ClientPlayer";
    public GameObject playerObject;  // El GameObject del cliente
    public GameObject hostPlayerPrefab;  // Prefab que representa al host en la escena
    private GameObject hostPlayerObject; // GameObject del host instanciado

    private bool isConnected = false;

    void Start()
    {
        Application.runInBackground = true;

        ConnectToServer("127.0.0.1", 9050); // Dirección del servidor
    }

    public void ConnectToServer(string serverIP, int port)
    {
        serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), port);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        // Enviar nombre del cliente al host
        PlayerData initialData = new PlayerData { playerName = playerName, position = Vector3.zero, command = "JOIN" };
        string json = JsonUtility.ToJson(initialData);
        Debug.Log($"Enviando mensaje JOIN: {json}");
        SendData(json);


        Debug.Log("Cliente conectado al servidor");

        isConnected = true;
        BeginReceive();
    }

    private void BeginReceive()
    {
        byte[] buffer = new byte[1024];
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remoteEndPoint, (ar) =>
        {
            int recv = socket.EndReceiveFrom(ar, ref remoteEndPoint);
            string receivedMessage = Encoding.ASCII.GetString(buffer, 0, recv);

            HandleMessage(receivedMessage);
            BeginReceive();
        }, null);
    }

    private void HandleMessage(string message)
    {
        PlayerData hostData = JsonUtility.FromJson<PlayerData>(message);

        if (hostData.command == "UPDATE")
        {
            // Instanciar al host en la escena del cliente si no se ha hecho ya
            if (hostPlayerObject == null && hostPlayerPrefab != null)
            {
                hostPlayerObject = Instantiate(hostPlayerPrefab, Vector3.zero, Quaternion.identity);
                hostPlayerObject.name = hostData.playerName;
            }

            // Actualizar la posición del host
            if (hostPlayerObject != null)
            {
                hostPlayerObject.transform.position = hostData.position;
                hostPlayerObject.transform.rotation = hostData.rotation;
            }
            Debug.Log($"Actualización de la posición del host: {hostData.position}");
        }
    }

    public void SendMovementAndRotation(Vector3 pos, Quaternion rot)
    {
        if (isConnected)
        {
            PlayerData playerData = new PlayerData
            {
                playerName = playerName,
                position = pos,
                rotation = rot,
                command = "MOVE"
            };
            SendData(JsonUtility.ToJson(playerData));
        }
    }

    private void SendData(string data)
    {
        byte[] buffer = Encoding.ASCII.GetBytes(data);
        socket.SendTo(buffer, buffer.Length, SocketFlags.None, serverEndPoint);
    }

    void OnApplicationQuit()
    {
        if (socket != null) socket.Close();
    }
}
