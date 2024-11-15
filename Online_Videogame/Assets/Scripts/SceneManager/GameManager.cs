using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TMP_InputField hostNameInputField;    // Nombre del jugador
    public TMP_InputField clientNameInputField;    // Nombre del jugador
    public TMP_InputField ipInputField;      // IP del servidor (solo para Cliente)
    public TextMeshProUGUI errorMessageTextHost;  // Mensaje de error para Host
    public TextMeshProUGUI errorMessageTextClient; // Mensaje de error para Cliente

    public GameConfigSO gameConfig;

    // Canvas para Host y Cliente
    public GameObject hostCanvas;
    public GameObject clientCanvas;

    private void Start()
    {
        // Aseguramos que ambos Canvas estén desactivados al principio
        hostCanvas.SetActive(false);
        clientCanvas.SetActive(false);

        // Limpiar mensajes de error
        if (errorMessageTextHost != null) errorMessageTextHost.text = "";
        if (errorMessageTextClient != null) errorMessageTextClient.text = "";
    }

    // Al hacer clic en "Create Server" (Host)
    public void StartHost()
    {
        hostCanvas.SetActive(false);  // Desactivar el Canvas del Host
        clientCanvas.SetActive(false);  // Desactivar el Canvas del Cliente
        ShowErrorMessageHost("");  // Limpiar el mensaje de error

        // Validar nombre
        if (string.IsNullOrWhiteSpace(hostNameInputField.text))
        {
            ShowErrorMessageHost("Por favor, introduce un nombre de usuario.");
            hostCanvas.SetActive(true);  // Volver a mostrar el Canvas del Host
            return;
        }

        // Establecer nombre en GameConfigSO
        gameConfig.SetPlayerName(hostNameInputField.text);
        gameConfig.SetRole("Host", true);

        Debug.Log("Starting as Host");
        GoToGameScene();
    }

    // Al hacer clic en "Join Game" (Cliente)
    public void JoinServer()
    {
        hostCanvas.SetActive(false);  // Desactivar el Canvas del Host
        clientCanvas.SetActive(false);  // Desactivar el Canvas del Cliente
        ShowErrorMessageClient("");  // Limpiar el mensaje de error

        // Validar nombre
        if (string.IsNullOrWhiteSpace(clientNameInputField.text))
        {
            ShowErrorMessageClient("Por favor, introduce un nombre de usuario.");
            clientCanvas.SetActive(true);  // Volver a mostrar el Canvas del Cliente
            return;
        }

        // Establecer nombre en GameConfigSO
        gameConfig.SetPlayerName(clientNameInputField.text);

        // Validar IP solo si es Cliente
        if (string.IsNullOrWhiteSpace(ipInputField.text))
        {
            ShowErrorMessageClient("Por favor, introduce una IP a la que conectarte.");
            clientCanvas.SetActive(true);  // Volver a mostrar el Canvas del Cliente
            return;
        }

        // Establecer IP en GameConfigSO
        gameConfig.SetPlayerIP(ipInputField.text);
        gameConfig.SetRole("Client", false);

        Debug.Log("Starting as Client");
        GoToGameScene();
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

    // Cargar la escena del juego
    private void GoToGameScene()
    {
        Debug.Log("Starting...!");
        SceneManager.LoadScene("GameScene");
    }
}
