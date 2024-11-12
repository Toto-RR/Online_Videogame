using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System.Collections.Generic;

public class ServerUDP : MonoBehaviour
{
    Socket socket;
    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string serverText;

    public TMP_InputField inputField;

    private Thread receiveThread;
    private bool isServerRunning = false;
    private List<Client> clients = new List<Client>();

    public struct Client
    {
        public string name;
        public EndPoint endPoint;
        public DateTime lastPing; // Time of the last received message
    }

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();
    }

    public void StartServer()
    {
        if (isServerRunning)
        {
            serverText += "\nServer is already running.";
            return;
        }

        string localIP = GetLocalIPAddress();
        serverText = "Starting UDP Server..." + "\nLocalIP:" + localIP;

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        // Allow reuse of the address to avoid socket exceptions
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        receiveThread = new Thread(Receive);
        receiveThread.Start();

        isServerRunning = true;
    }

    string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }

    void Update()
    {
        UItext.text = serverText;
    }

    void Receive()
    {
        byte[] data = new byte[1024];
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint remoteClient = (EndPoint)sender;

        serverText += "\nWaiting for new Clients...";

        while (isServerRunning)
        {
            try
            {
                int recv = socket.ReceiveFrom(data, ref remoteClient);
                string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

                // Handle disconnection messages
                if (receivedMessage.StartsWith("LEAVE:"))
                {
                    string clientName = receivedMessage.Substring(6);
                    RemoveClient(remoteClient, clientName);
                    continue;
                }

                // Check if it's a new client
                Client existingClient = clients.Find(c => c.endPoint.Equals(remoteClient));
                if (existingClient.name == null)
                {
                    Client newClient = new Client { name = receivedMessage, endPoint = remoteClient, lastPing = DateTime.Now };
                    clients.Add(newClient);

                    serverText += $"\nNew client connected: {newClient.name}";

                    // Send welcome message only to the new client
                    SendToClient($"SERVER_NAME:\nWelcome {newClient.name} to {UserData.username}'s room!", remoteClient);

                    // Notify other clients that a new client has joined
                    BroadcastMessage($"{newClient.name} has joined the waiting room", remoteClient);
                }
                else
                {
                    existingClient.lastPing = DateTime.Now; // Update ping time
                    serverText += $"\n{receivedMessage}";
                    BroadcastMessage($"{receivedMessage}", remoteClient);
                }
            }
            catch (SocketException e)
            {
                serverText += $"\nSocket exception: {e.Message}";
                break;
            }
        }
    }

    void RemoveClient(EndPoint clientEndPoint, string clientName)
    {
        Client clientToRemove = clients.Find(c => c.endPoint.Equals(clientEndPoint));
        if (clientToRemove.name != null)
        {
            clients.Remove(clientToRemove);
            serverText += $"\nClient {clientName} has disconnected.";
        }
    }

    public void StopServer()
    {
        if (!isServerRunning) return;

        Debug.Log("SERVER_CLOSING: The server is shutting down.", null);

        isServerRunning = false;

        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Interrupt(); // Interrupt the thread to stop blocking calls
            receiveThread.Join(1000); // Wait for the thread to finish (max 1 second)
        }

        if (socket != null)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both); // Gracefully shut down the socket
            }
            catch (SocketException e)
            {
                Debug.LogWarning($"Error during socket shutdown: {e.Message}");
            }

            socket.Close(); // Finally close the socket
            socket = null;
        }

        Debug.Log("\nUDP Server stopped.");
    }

    // This method sends a message from the server to a specific client or to all clients
    public void SendMessageToClient()
    {
        if (inputField.text != "")
        {
            if (!isServerRunning)
            {
                serverText += "\nServer is not running.";
                return;
            }

            string message = $"{UserData.username}: {inputField.text}";

            // Broadcast to all clients
            BroadcastMessage(message, null);
            Debug.Log($"\nMessage broadcasted to all clients: {inputField.text}");

            serverText += $"\n{message}";

            inputField.text = ""; // Clear the input field after sending
        }
    }

    void BroadcastMessage(string message, EndPoint senderClient)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);

        foreach (Client client in clients)
        {
            if (senderClient == null || !client.endPoint.Equals(senderClient))
            {
                socket.SendTo(data, data.Length, SocketFlags.None, client.endPoint);
            }
        }
    }

    void SendToClient(string message, EndPoint clientEndPoint)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        socket.SendTo(data, data.Length, SocketFlags.None, clientEndPoint);
    }

    void OnApplicationQuit()
    {
        StopServer();
    }
}
