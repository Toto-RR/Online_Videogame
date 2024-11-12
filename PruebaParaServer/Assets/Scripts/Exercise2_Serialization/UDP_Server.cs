using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string playerName;
    public Vector3 position;
    public string command;
}

[Serializable]
public class NPCData
{
    public string npcName;
    public Vector3 position;
}

public class UDP_Server : MonoBehaviour
{
    private Socket socket;
    private List<Client> clients = new List<Client>();
    private bool isServerRunning = false;

    public struct Client
    {
        public string name;
        public Vector3 position;
        public EndPoint endPoint;
    }

    void Start()
    {
        StartServer();
    }

    public void StartServer()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        Debug.Log("Server started...");
        isServerRunning = true;
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

            // Procesar mensaje
            HandleMessage(receivedMessage, remoteEndPoint);

            // Continuar recibiendo
            BeginReceive();
        }, null);
    }

    private void HandleMessage(string message, EndPoint clientEndPoint)
    {
        PlayerData playerData = JsonUtility.FromJson<PlayerData>(message);

        // Verificar si el cliente ya existe
        Client client = clients.Find(c => c.name == playerData.playerName);
        if (client.name == null)
        {
            // Nuevo cliente
            Client newClient = new Client { name = playerData.playerName, position = playerData.position, endPoint = clientEndPoint };
            clients.Add(newClient);
            Debug.Log($"New client connected: {newClient.name}");
        }
        else
        {
            // Actualizar posición y comandos del cliente
            if (playerData.command == "MOVE")
            {
                client.position = playerData.position;
            }
            else if (playerData.command == "JUMP")
            {
                // Manejo de salto (se podría añadir lógica específica para la física del salto si es necesario)
                Debug.Log($"{client.name} has jumped.");
            }
        }

        // Enviar estado del juego
        BroadcastGameState();
    }

    private void BroadcastGameState()
    {
        foreach (Client client in clients)
        {
            // Enviar posiciones de todos los jugadores a cada cliente
            foreach (Client otherClient in clients)
            {
                PlayerData playerData = new PlayerData
                {
                    playerName = otherClient.name,
                    position = otherClient.position,
                    command = "UPDATE"
                };
                string json = JsonUtility.ToJson(playerData);
                byte[] data = Encoding.ASCII.GetBytes(json);
                socket.SendTo(data, data.Length, SocketFlags.None, client.endPoint);
            }

            // Enviar posición del NPC (ejemplo básico)
            NPCData npcData = new NPCData
            {
                npcName = "NPC_1",
                position = new Vector3(5, 0, 5) // Ejemplo de posición fija para el NPC
            };
            string npcJson = JsonUtility.ToJson(npcData);
            byte[] npcDataBytes = Encoding.ASCII.GetBytes(npcJson);
            socket.SendTo(npcDataBytes, npcDataBytes.Length, SocketFlags.None, client.endPoint);
        }
    }

    void OnApplicationQuit()
    {
        if (socket != null) socket.Close();
    }
}
