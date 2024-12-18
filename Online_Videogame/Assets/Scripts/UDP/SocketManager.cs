using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class SocketManager : MonoBehaviour
{
    public static SocketManager Instance { get; private set; }

    private Socket socket;
    public EndPoint ServerEndPoint { get; private set; }

    private bool isServer;

    public bool IsConnected => socket != null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeServer(int port)
    {
        if (socket == null)
        {
            isServer = true;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
            socket.Bind(ipep); // Asociar el socket al puerto
            Debug.Log($"Server socket bound to port {port}.");
        }
    }

    public void InitializeClient(string serverIP, int serverPort)
    {
        if (socket == null)
        {
            isServer = false;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ServerEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
            Debug.Log($"Client socket created to connect to {serverIP}:{serverPort}.");
        }
    }

    public Socket GetSocket()
    {
        return socket;
    }

    public void CloseSocket()
    {
        if (socket != null)
        {
            socket.Close();
            socket = null;
            Debug.Log("Socket closed.");
        }
    }
}
