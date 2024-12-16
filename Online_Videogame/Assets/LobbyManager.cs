using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    // Lobby
    private List<string> playerNamesList = new List<string>();
    public TextMeshProUGUI playerNamesText;
    public Button startOrReadyButton;
    public Transform playerPrefabParent;
    public GameObject playerPrefab;

    // Game Flow
    public GameConfigSO gameConfig;
    public UDP_Server udpServer;
    public UDP_Client udpClient;

    private GameObject playerInstance;
    private TextMeshProUGUI buttonText;
    private bool isHost;

    void Start()
    {
        // Inicializar roles y UI
        isHost = gameConfig.PlayerRole == "Host";

        if (startOrReadyButton != null)
        {
            buttonText = startOrReadyButton.GetComponentInChildren<TextMeshProUGUI>();
            ConfigureButton();
        }

        // Instanciar el modelo del jugador
        if (playerPrefab != null && playerPrefabParent != null)
        {
            playerInstance = Instantiate(playerPrefab, playerPrefabParent);
        }
    }

    void Update()
    {
        // Rotar el modelo del jugador
        if (playerInstance != null)
        {
            playerInstance.transform.Rotate(0, 50 * Time.deltaTime, 0);
        }
    }

    // Configura el botón dependiendo del rol
    private void ConfigureButton()
    {
        if (isHost)
        {
            buttonText.text = "START GAME";
            startOrReadyButton.onClick.AddListener(StartGame);
        }
        else
        {
            buttonText.text = "READY";
            startOrReadyButton.onClick.AddListener(SetReady);
        }
    }

    // Añadir jugador al lobby
    public void JoinPlayer(PlayerData newPlayer)
    {
        if (newPlayer == null || string.IsNullOrWhiteSpace(newPlayer.PlayerName)) return;

        if (!playerNamesList.Contains(newPlayer.PlayerName))
        {
            playerNamesList.Add(newPlayer.PlayerName);
            UpdatePlayerNamesDisplay();
        }
    }

    private void UpdatePlayerNamesDisplay()
    {
        playerNamesText.text = string.Join("\n", playerNamesList);
    }

    // Si el jugador es cliente, envía READY al servidor
    private void SetReady()
    {
        if (udpClient != null)
        {
            udpClient.SendReadyMessage();
            buttonText.text = "WAITING...";
            startOrReadyButton.interactable = false;
        }
    }

    // Si el jugador es host, verifica si todos están listos e inicia el juego
    private void StartGame()
    {
        if (!isHost) return;

        if (udpServer != null && AreAllPlayersReady())
        {
            udpServer.SetGameState(GameState.ServerState.IN_GAME);
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogWarning("No todos los jugadores están listos.");
            ShowMessage("No todos los jugadores están listos.");
        }
    }

    private bool AreAllPlayersReady()
    {
        foreach (var player in udpServer.GetPlayers())
        {
            Debug.Log(player.PlayerId);
            Debug.Log(gameConfig.PlayerID);
            if (player.PlayerId != gameConfig.PlayerID && !player.IsReady)
            {
                return false;
            }
        }
        return true;
    }

    // Mostrar mensajes en la UI
    public void ShowMessage(string message)
    {
        // Implementa esto si necesitas mostrar mensajes al jugador
        Debug.Log(message);
    }
}
