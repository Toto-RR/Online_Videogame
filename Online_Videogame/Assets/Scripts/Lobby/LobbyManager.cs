using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    private UDP_Client udpClient;
    private UDP_Server udpServer;

    public GameConfigSO gameConfig;

    public GameObject playerPrefab; // Modelo visual del jugador
    private GameObject playerObject;
    public Transform playerParent;

    public Button startGameButton; // Solo visible para el Host
    public Button readyButton;     // Visible para los clientes

    public TextMeshProUGUI playerNames; // UI para la lista de jugadores
    private List<LobbyPlayerData> lobbyPlayers = new List<LobbyPlayerData>();

    public static LobbyManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        udpClient = GetComponentInChildren<UDP_Client>();
        udpServer = GetComponentInChildren<UDP_Server>();

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

        Debug.Log("Lobby UI Updated.");
    }

    // Enviar READY al servidor (Clientes)
    private void OnReadyPressed()
    {
        PlayerSync.Instance.SendReadyRequest();
        Debug.Log("READY request sent.");
    }

    // Enviar START_GAME al servidor (Host)
    private void OnStartGamePressed()
    {
        PlayerSync.Instance.SendJoinGameRequest();
        Debug.Log("START GAME request sent.");
    }
}
