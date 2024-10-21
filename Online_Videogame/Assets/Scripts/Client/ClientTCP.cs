using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using System.Text;
using UnityEngine.UI;
using System;

public class ClientTCP : MonoBehaviour
{
    public GameObject UItextObj;
    private TextMeshProUGUI UItext;
    private string clientText;
    private Socket server;
    private string serverName;

    //Text input for enter IP
    public TMP_InputField ipInputField;

    //Text input for type messages
    public TMP_InputField inputField;

    private bool isConnected = false;

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();
        //clientText = "Connecting...";
        //UItext.text = clientText;
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

        Thread connect = new Thread(Connect);
        connect.Start();
    }

    void Connect()
    {
        if (!string.IsNullOrEmpty(ipInputField.text))
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ipInputField.text), 9050);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                server.Connect(ipep);
                Thread sendThread = new Thread(Send);
                sendThread.Start();

                Thread receiveThread = new Thread(Receive);
                receiveThread.Start();

                isConnected = true;
            }
            catch (SocketException)
            {
                clientText += "\nServer not found";
            }
        }
        else
        {
            clientText += "\nPlease, enter a valid IP";
        }
    }


    void Send()
    {
        string message = UserData.username;
        byte[] data = Encoding.ASCII.GetBytes(message);

        server.Send(data);
        clientText += "Username sent: " + message;
    }

    void Receive()
    {
        byte[] data = new byte[1024];

        // First, receive the server's name
        int serverNameLength = server.Receive(data);
        serverName = Encoding.ASCII.GetString(data, 0, serverNameLength);
        clientText += "\nConnected to server: " + serverName;

        while (true)
        {
            try
            {
                int recv = server.Receive(data);
                if (recv > 0)
                {
                    string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);
                    clientText += $"\n{receivedMessage}";
                }
            }
            catch (SocketException)
            {
                clientText += "\nDisconnected from server.";
                UItextObj.SetActive(false);
                break;
            }
        }
    }

    public void SendMessageToServer()
    {
        if (!isConnected)
        {
            clientText += "\nYou are not connected to any server.";
            return;
        }

        if (inputField.text != "")
        {
            string message = inputField.text;
            byte[] data = Encoding.ASCII.GetBytes(message);

            try
            {
                server.Send(data);
                clientText += "\nYou: " + message;
                inputField.text = "";
            }
            catch (SocketException ex)
            {
                clientText += $"\nError sending message: {ex.Message}";
            }
        }
    }

}
