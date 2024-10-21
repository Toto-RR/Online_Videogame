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
            socket.Close(); // Asegurarse de que el socket anterior esté cerrado
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

            SendInitialMessage();

            isConnected = true;

            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
        }
        else
        {
            errorMessageText.text = "Please, enter the server IP.";
        }
    }

    void SendInitialMessage()
    {
        string message = UserData.username;
        byte[] data = Encoding.ASCII.GetBytes(message);

        socket.SendTo(data, data.Length, SocketFlags.None, serverEndPoint);
        //clientText += "Username sent: " + message;
    }

    void ReceiveMessages()
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint remoteServer = (EndPoint)sender;
        byte[] data = new byte[1024];

        // Recibir el primer mensaje (mensaje de bienvenida)
        int recv = socket.ReceiveFrom(data, ref remoteServer);
        string initialMessage = Encoding.ASCII.GetString(data, 0, recv);

        if (initialMessage.StartsWith("SERVER_NAME:"))
        {
            serverName = initialMessage.Substring("SERVER_NAME:".Length);
            clientText += $"Connected to server: {serverName}";
        }

        // Bucle para recibir mensajes continuos
        while (true)
        {
            try
            {
                recv = socket.ReceiveFrom(data, ref remoteServer);
                if (recv > 0)
                {
                    string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

                    // No añadir el serverName en cada mensaje recibido
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
        DisconnectFromServer(); // Llamamos a la función de desconexión al cerrar la aplicación
    }

    public void DisconnectFromServer()
    {
        if (isConnected)
        {
            if (socket != null)
            {
                try
                {
                    // Enviar mensaje de desconexión al servidor
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
                    socket.Close();  // Cerrar el socket
                    socket = null;
                }
            }

            isConnected = false; // Marcar como desconectado
            clientText += "\nDisconnected from server.";
        }
    }

}
