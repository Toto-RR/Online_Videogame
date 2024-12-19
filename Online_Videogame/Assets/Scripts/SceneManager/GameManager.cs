using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class GameManager : MonoBehaviour
{
    public TMP_InputField hostNameInputField;    // Player name
    public TMP_InputField clientNameInputField;    // Player ID
    public TMP_InputField ipInputField;      // Server IP (only client)
    public TextMeshProUGUI errorMessageTextHost;  // Host error message
    public TextMeshProUGUI errorMessageTextClient; // Client error message

    public GameConfigSO gameConfig;

    private void Start()
    {
        if (errorMessageTextHost != null) errorMessageTextHost.text = "";
        if (errorMessageTextClient != null) errorMessageTextClient.text = "";
    }

    public void StartHost()
    {
        // Clean the error message
        ShowErrorMessageHost("");

        // Valid name
        if (string.IsNullOrWhiteSpace(hostNameInputField.text))
        {
            ShowErrorMessageHost("Please, enter a valid username.");
            return;
        }

        // Set parameters on GameConfigSO
        gameConfig.SetPlayerName(hostNameInputField.text);
        gameConfig.SetID(Guid.NewGuid().ToString());
        gameConfig.SetRole("Host");

        Debug.Log("Starting as Host");
        GoToGameScene();
    }

    public void JoinServer()
    {
        ShowErrorMessageClient("");

        // Valid name
        if (string.IsNullOrWhiteSpace(clientNameInputField.text))
        {
            ShowErrorMessageClient("Please, enter a valid username.");
            return;
        }

        // Valid IP
        if (string.IsNullOrWhiteSpace(ipInputField.text))
        {
            ShowErrorMessageClient("Please, enter a valid IP.");
            return;
        }

        // Set parameters on GameConfigSO
        gameConfig.SetPlayerName(clientNameInputField.text);
        gameConfig.SetID(Guid.NewGuid().ToString());
        gameConfig.SetPlayerIP(ipInputField.text);
        gameConfig.SetRole("Client");

        if (CheckConexion(gameConfig.PlayerIP, 9050))
        {
            Debug.Log("Starting as Client");
            GoToGameScene();

        }
        else
        {
            ShowErrorMessageClient("Could not connect to the server. Please check the IP or try again later.");
        }

    }

    private void ShowErrorMessageHost(string message)
    {
        if (errorMessageTextHost != null)
        {
            errorMessageTextHost.text = message;
        }
    }

    private void ShowErrorMessageClient(string message)
    {
        if (errorMessageTextClient != null)
        {
            errorMessageTextClient.text = message;
        }
    }

    private bool CheckConexion(string serverIP, int port)
    {
        bool isServerAvailable = false;
        using (Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            try
            {
                udpSocket.SendTimeout = 1000; // Limit time to send (1 segundo)
                udpSocket.ReceiveTimeout = 1000; // Limit time to response (1 segundo)

                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), port);

                // Test message ("ping")
                string pingMessage = "ping";
                byte[] pingData = Encoding.UTF8.GetBytes(pingMessage);
                udpSocket.SendTo(pingData, serverEndPoint);


                // Try to get answer
                byte[] responseBuffer = new byte[1024];
                EndPoint serverResponseEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int receivedBytes = udpSocket.ReceiveFrom(responseBuffer, ref serverResponseEndPoint);

                // Process if is available
                string responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, receivedBytes);
                if (responseMessage == "pong") // The server must respond “pong”
                {
                    isServerAvailable = true;
                }
            }
            catch (SocketException ex)
            {
                Debug.LogError($"Error al verificar conexión con el servidor: {ex.Message}");
            }
        }

        return isServerAvailable;
    }

    private void GoToGameScene()
    {
        Debug.Log("ID: " + gameConfig.PlayerID);
        Debug.Log("Name: " + gameConfig.PlayerName);
        Debug.Log("Starting...!");
        SceneManager.LoadScene("Lobby");
    }
}
