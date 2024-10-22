using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;

public class ClientUDP : MonoBehaviour
{
    Socket socket;
    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string clientText;
    string serverName;

    public TMP_InputField ipInputField;
    public TextMeshProUGUI errorMessageText;
    public TMP_InputField inputField;

    IPEndPoint serverEndPoint;

    private bool isConnected = false;

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        UItext.text = clientText;
    }

    public void StartClient()
    {
        if (isConnected)
        {
            clientText += "\nYou are already connected";
            return;
        }

        if (socket != null)
        {
            socket.Close(); // Make sure that the previous socket is closed
        }

        Thread connectThread = new Thread(Connect);
        connectThread.Start();
    }

    void Connect()
    {
        if (!string.IsNullOrEmpty(ipInputField.text))
        {
            serverEndPoint = new IPEndPoint(IPAddress.Parse(ipInputField.text), 9050);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                // Send initial message to check if server is active
                SendInitialMessage();

                byte[] data = new byte[1024];
                EndPoint remoteServer = new IPEndPoint(IPAddress.Any, 0);

                // If the connection is successful, mark as connected
                int recv = socket.ReceiveFrom(data, ref remoteServer);
                string initialMessage = Encoding.ASCII.GetString(data, 0, recv);

                if (initialMessage.StartsWith("SERVER_NAME:"))
                {
                    serverName = initialMessage.Substring("SERVER_NAME:".Length);
                    clientText += $"{serverName}";

                    // If the connection is successful, mark as connected
                    isConnected = true;

                    // Start the thread to receive messages
                    Thread receiveThread = new Thread(ReceiveMessages);
                    receiveThread.Start();
                }
            }
            catch (SocketException)
            {
                clientText += "\nServer not found";

                // Make sure to close the socket if there is an error
                if (socket != null)
                {
                    socket.Close();
                    socket = null;
                }
            }
        }
        else
        {
            clientText += "Please, enter the server IP.";
        }
    }


    void SendInitialMessage()
    {
        string message = UserData.username;
        byte[] data = Encoding.ASCII.GetBytes(message);

        socket.SendTo(data, data.Length, SocketFlags.None, serverEndPoint);
    }

    void ReceiveMessages()
    {
        if (!isConnected) return;

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint remoteServer = (EndPoint)sender;
        byte[] data = new byte[1024];

        // Receiving the first message (welcome message)
        int recv = socket.ReceiveFrom(data, ref remoteServer);
        string initialMessage = Encoding.ASCII.GetString(data, 0, recv);

        if (initialMessage.StartsWith("SERVER_NAME:"))
        {
            serverName = initialMessage.Substring("SERVER_NAME:".Length);
            clientText += $"Connected to server: {serverName}";
        }

        // Loop to receive continuous messages
        while (true)
        {
            try
            {
                recv = socket.ReceiveFrom(data, ref remoteServer);
                if (recv > 0)
                {
                    string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

                    // Do not add the serverName to each message received
                    clientText += $"\n{receivedMessage}";
                }
            }
            catch (SocketException)
            {
                clientText += "\nDisconnected from server.";
                break;
            }
        }
    }


    public void SendMessageToServer()
    {
        if (!isConnected)
        {
            clientText += "\nYou are not connected to a server. Please connect first.";
            return;
        }

        if (inputField.text != "")
        {
            string message = $"{UserData.username}: " + inputField.text;
            byte[] data = Encoding.ASCII.GetBytes(message);

            socket.SendTo(data, data.Length, SocketFlags.None, serverEndPoint);
            clientText += $"\n{UserData.username}: " + inputField.text;
            inputField.text = "";
        }
    }

    void OnApplicationQuit()
    {
        DisconnectFromServer(); // We call the shutdown function when closing the application.
    }

    public void DisconnectFromServer()
    {
        if (isConnected)
        {
            if (socket != null)
            {
                try
                {
                    // Send disconnection message to server
                    string disconnectMessage = $"LEAVE:{UserData.username}";
                    byte[] data = Encoding.ASCII.GetBytes(disconnectMessage);
                    socket.SendTo(data, data.Length, SocketFlags.None, serverEndPoint);
                }
                catch (SocketException ex)
                {
                    clientText += $"\nError while disconnecting: {ex.Message}";
                }
                finally
                {
                    socket.Close();  // Close the socket
                    socket = null;
                }
            }

            isConnected = false; // Mark as offline
            clientText += "\nDisconnected from server.";
        }
    }

}
