using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class GameManager : MonoBehaviour
{
    public TMP_InputField hostNameInputField;    // Nombre del jugador
    public TMP_InputField clientNameInputField;    // Nombre del jugador
    public TMP_InputField ipInputField;      // IP del servidor (solo para Cliente)
    public TextMeshProUGUI errorMessageTextHost;  // Mensaje de error para Host
    public TextMeshProUGUI errorMessageTextClient; // Mensaje de error para Cliente

    public GameConfigSO gameConfig;

    private void Start()
    {
        // Aseguramos que ambos Canvas estén desactivados al principio
        //hostCanvas.SetActive(false);
        //clientCanvas.SetActive(false);

        // Limpiar mensajes de error
        if (errorMessageTextHost != null) errorMessageTextHost.text = "";
        if (errorMessageTextClient != null) errorMessageTextClient.text = "";
    }

    // Al hacer clic en "Create Server" (Host)
    public void StartHost()
    {
        //hostCanvas.SetActive(false);  // Desactivar el Canvas del Host
        //clientCanvas.SetActive(false);  // Desactivar el Canvas del Cliente
        ShowErrorMessageHost("");  // Limpiar el mensaje de error

        // Validar nombre
        if (string.IsNullOrWhiteSpace(hostNameInputField.text))
        {
            ShowErrorMessageHost("Por favor, introduce un nombre de usuario.");
            return;
        }

        // Establecer parámetros en GameConfigSO
        gameConfig.SetPlayerName(hostNameInputField.text);
        gameConfig.SetID(Guid.NewGuid().ToString());
        gameConfig.SetRole("Host");

        Debug.Log("Starting as Host");
        GoToLobby();
    }

    // Al hacer clic en "Join Game" (Cliente)
    public void JoinServer()
    {
        //hostCanvas.SetActive(false);  // Desactivar el Canvas del Host
        //clientCanvas.SetActive(false);  // Desactivar el Canvas del Cliente
        ShowErrorMessageClient("");  // Limpiar el mensaje de error

        // Validar nombre
        if (string.IsNullOrWhiteSpace(clientNameInputField.text))
        {
            ShowErrorMessageClient("Please, enter a valid username.");
            return;
        }

        // Establecer parámetros en GameConfigSO
        gameConfig.SetPlayerName(clientNameInputField.text);
        gameConfig.SetID(Guid.NewGuid().ToString());

        // Validar IP solo si es Cliente
        if (string.IsNullOrWhiteSpace(ipInputField.text))
        {
            ShowErrorMessageClient("Please, enter a valid IP.");
            return;
        }

        // Establecer IP en GameConfigSO
        gameConfig.SetPlayerIP(ipInputField.text);
        gameConfig.SetRole("Client");

        if (CheckConexion(gameConfig.PlayerIP, 9050))
        {
            Debug.Log("Starting as Client");
            GoToLobby();
        }
        else
        {
            ShowErrorMessageClient("Could not connect to the server. Please check the IP or try again later.");
        }

    }

    // Mostrar mensaje de error para Host
    private void ShowErrorMessageHost(string message)
    {
        if (errorMessageTextHost != null)
        {
            errorMessageTextHost.text = message;
        }
    }

    // Mostrar mensaje de error para Cliente
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

                // Mensaje de prueba (puede ser un simple "ping")
                string pingMessage = "ping";
                byte[] pingData = Encoding.UTF8.GetBytes(pingMessage);
                udpSocket.SendTo(pingData, serverEndPoint);


                // Try to get answer
                byte[] responseBuffer = new byte[1024];
                EndPoint serverResponseEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int receivedBytes = udpSocket.ReceiveFrom(responseBuffer, ref serverResponseEndPoint);

                // Process if is available
                string responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, receivedBytes);
                if (responseMessage == "pong") // El servidor debe responder "pong"
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


    // Cargar la escena del juego
    private void GoToLobby()
    {
        Debug.Log("ID: " + gameConfig.PlayerID);
        Debug.Log("Name: " + gameConfig.PlayerName);
        Debug.Log("Starting...!");
        SceneManager.LoadScene("Lobby");
    }
}
