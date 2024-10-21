using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using System.Text;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using System;

public class ServerTCP : MonoBehaviour
{
    private Socket socket;
    private Thread mainThread = null;
    private bool serverRunning = false;

    public GameObject UItextObj;
    private TextMeshProUGUI UItext;
    private string serverText;

    //Text input for type messages
    public TMP_InputField inputField;

    //Connected users list to the waiting room
    private List<User> connectedUsers = new List<User>();

    public struct User
    {
        public string name;
        public Socket socket;
    }

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        UItext.text = serverText;
    }

    public void startServer()
    {
        if (serverRunning)
        {
            serverText = "Server already running. Restarting...";
            StopServer();
        }
        else
        {
            string localIP = GetLocalIPAddress();
            serverText = "Starting TCP Server..." + "\nLocalIP:" + localIP;

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 9050);
            socket.Bind(localEndPoint);
            socket.Listen(10);

            //Allow the socket to reuse the address
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            serverRunning = true;

            mainThread = new Thread(CheckNewConnections);
            mainThread.Start();
        }
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
    public void StopServer()
    {
        if (socket != null && socket.Connected)
        {
            foreach (User user in connectedUsers)
            {
                try
                {
                    user.socket.Shutdown(SocketShutdown.Both);
                    user.socket.Close();
                }
                catch (SocketException ex)
                {
                    serverText += "\nError closing client socket: " + ex.Message;
                }
            }

            connectedUsers.Clear();
            socket.Close();
            socket = null;
            serverText += "\nServer stopped.";
            serverRunning = false;
        }
    }
    void CheckNewConnections()
    {
        while (true)
        {
            User newUser = new User();
            newUser.socket = socket.Accept();

            //Check the username
            byte[] nameData = new byte[1024];
            int nameLength = newUser.socket.Receive(nameData);
            newUser.name = Encoding.ASCII.GetString(nameData, 0, nameLength);

            IPEndPoint clientEndPoint = (IPEndPoint)newUser.socket.RemoteEndPoint;
            serverText += $"\nUser connected: {newUser.name} ({clientEndPoint.Address} at port {clientEndPoint.Port})";

            connectedUsers.Add(newUser); // Add the user to the list of connected users

            // Send server name to the connected user
            byte[] serverNameData = Encoding.ASCII.GetBytes(UserData.username);
            newUser.socket.Send(serverNameData);


            Thread newConnection = new Thread(() => Receive(newUser));
            newConnection.Start();
        }
    }

    void Receive(User user)
    {
        byte[] data = new byte[1024];
        int recv = 0;

        while (true)
        {
            try
            {
                recv = user.socket.Receive(data);
                if (recv == 0)
                {
                    // Cliente desconectado
                    serverText += $"\n{user.name} disconnected.";
                    connectedUsers.Remove(user);
                    user.socket.Shutdown(SocketShutdown.Both);
                    user.socket.Close();
                    break;  // Salir del bucle
                }
                else
                {
                    string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);
                    serverText += $"\n{user.name}: {receivedMessage}";

                    // Broadcast de mensaje
                    Thread answer = new Thread(() => Send(user, receivedMessage));
                    answer.Start();
                }
            }
            catch (SocketException ex)
            {
                // Manejar la desconexión
                serverText += $"\nError: {ex.Message}. {user.name} has disconnected.";
                connectedUsers.Remove(user);
                user.socket.Close();
                break;
            }
        }
    }


    // Esta funcion sirve para REENVIAR los mensajes de otros a todos los clientes
    void Send(User sender, string message)
    {
        string finalMessage = $"{sender.name}: {message}";
        byte[] data = Encoding.ASCII.GetBytes(finalMessage);

        foreach (User user in connectedUsers)
        {
            if (user.socket != sender.socket)
            {
                user.socket.Send(data);
            }
        }

        Debug.Log("Broadcasted message to all users.");
    }

    // Esta sirve para ENVIAR el mensaje que el servidor escriba
    public void SendMessageToClient()
    {
        if (inputField.text != "")
        {
            string message = inputField.text;

            //The message to send to 
            string messageToSend = $"{UserData.username}: {message}";
            byte[] data = Encoding.ASCII.GetBytes(messageToSend);

            serverText += "\nYou: " + message;
            

            foreach (User user in connectedUsers)
            {
                user.socket.Send(data);
                inputField.text = "";
            }
        }
    }
}
