using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public UDP_Client udpClient;
    public UDP_Server udpServer;

    public GameConfigSO gameConfig;

    public GameObject playerPrefab; // Modelo visual del jugador
    private GameObject playerObject;
    public Transform playerParent;

    public Button startGameButton; // Solo visible para el Host
    public Button readyButton;     // Visible para los clientes

    public TextMeshProUGUI playerNames; // UI para la lista de jugadores
    private List<LobbyPlayerData> lobbyPlayers = new List<LobbyPlayerData>();

    private LobbyState lobbyState;
    public static LobbyManager Instance;

    private Renderer playerRenderer;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (DetermineIfHost())
        {
            udpServer.enabled = true;
            udpClient.enabled = false;

            startGameButton.enabled = true;
            readyButton.enabled = false;
            readyButton.transform.gameObject.SetActive(false);
        }
        else
        {
            udpClient.enabled = true;
            udpServer.enabled = false;

            readyButton.enabled = true;
            startGameButton.enabled = false;
            startGameButton.transform.gameObject.SetActive(false);
        }

        // Instanciar el modelo del jugador en el lobby
        if (playerPrefab != null && playerParent != null)
        {
            playerObject = Instantiate(playerPrefab, playerParent);
            playerRenderer = playerObject.GetComponentInChildren<Renderer>();
        }

        playerNames.text = ""; // Inicializar UI
    }

    private void Update()
    {
        // Efecto de rotación del modelo del jugador
        if (playerObject != null)
        {
            playerObject.transform.Rotate(0, 50 * Time.deltaTime, 0);
        }
    }
    public void OnColorChange(Color color)
    {
        if (playerRenderer != null)
        {
            playerRenderer.material.color = color;
        }
    }

    // Determinar si el jugador actual es el Host
    private bool DetermineIfHost()
    {
        return gameConfig.PlayerRole == "Host";
    }

    // Actualizar la UI del Lobby con los jugadores y su estado READY
    public void UpdateLobbyUI(List<LobbyPlayerData> players)
    {
        lobbyPlayers = players;
        playerNames.text = ""; // Limpiar la UI

        foreach (var player in players)
        {
            playerNames.text += $"{player.PlayerName} {(player.IsReady ? "(Ready)" : "")}\n";
        }
    }

    // Enviar READY al servidor (Clientes)
    public void OnReadyPressed()
    {
        PlayerSync.Instance.SendReadyRequest(playerRenderer.material.color);
        Debug.Log("READY request sent.");
    }

    // Enviar START_GAME al servidor (Host)
    public void OnStartGamePressed()
    {
        PlayerSync.Instance.SendReadyRequest(playerRenderer.material.color);
        PlayerSync.Instance.SendStartGameRequest();
        Debug.Log("START GAME request sent.");
    }
}
