using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDP_Client : MonoBehaviour
{
    private Socket socket;
    private EndPoint serverEndPoint;
    public string playerName = "Player1";
    public GameObject playerObject;
    public GameObject npcObject;
    public GameObject otherPlayerObject; // Objeto para mostrar el otro jugador

    private bool isConnected = false;

    void Start()
    {
        ConnectToServer("127.0.0.1", 9050); // Dirección del servidor y puerto
    }

    public void ConnectToServer(string serverIP, int port)
    {
        serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), port);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        // Enviar nombre del jugador
        PlayerData initialData = new PlayerData { playerName = playerName, position = Vector3.zero, command = "JOIN" };
        SendData(JsonUtility.ToJson(initialData));

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

            // Procesar mensaje recibido
            HandleMessage(receivedMessage);

            // Continuar recibiendo
            BeginReceive();
        }, null);
    }

    private void HandleMessage(string message)
    {
        if (message.Contains("npcName"))
        {
            NPCData npcData = JsonUtility.FromJson<NPCData>(message);
            UpdateNPCPosition(npcData);
        }
        else
        {
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(message);
            UpdatePlayerPosition(playerData);
        }
    }

    public void SendMovement(Vector3 position)
    {
        if (isConnected)
        {
            PlayerData playerData = new PlayerData
            {
                playerName = playerName,
                position = position,
                command = "MOVE"
            };
            SendData(JsonUtility.ToJson(playerData));
        }
    }

    public void SendMessage(string command)
    {
        if (isConnected)
        {
            PlayerData playerData = new PlayerData
            {
                playerName = playerName,
                position = playerObject.transform.position,
                command = command
            };
            SendData(JsonUtility.ToJson(playerData));
        }
    }

    private void SendData(string data)
    {
        byte[] buffer = Encoding.ASCII.GetBytes(data);
        socket.SendTo(buffer, buffer.Length, SocketFlags.None, serverEndPoint);
    }

    private void UpdatePlayerPosition(PlayerData playerData)
    {
        // Lógica para mover al jugador en el cliente con la posición recibida
        if (otherPlayerObject != null)
        {
            otherPlayerObject.transform.position = playerData.position;
        }
    }

    private void UpdateNPCPosition(NPCData npcData)
    {
        // Lógica para mover al NPC en el cliente con la posición recibida
        if (npcObject != null)
        {
            npcObject.transform.position = npcData.position;
        }
    }

    void OnApplicationQuit()
    {
        if (socket != null) socket.Close();
    }
}
