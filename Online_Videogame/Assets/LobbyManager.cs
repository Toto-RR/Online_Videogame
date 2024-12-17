using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    private UDP_Server udpServer;
    private UDP_Client udpClient;

    public GameObject serverObject;
    public GameObject clientObject;

    public GameObject playerPrefab;
    private GameObject playerObject;
    public Transform playerParent;

    public GameConfigSO gameConfig;

    public Button startGameButton;
    public Button readyButton;

    public TextMeshProUGUI playerNames;
    private Dictionary<string, EndPoint> playerNamesList = new Dictionary<string, EndPoint>();

    void Start()
    {
        udpServer = GetComponentInChildren<UDP_Server>();
        udpClient = GetComponentInChildren<UDP_Client>();

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

        // Instanciar el modelo del jugador
        if (playerPrefab != null && playerParent != null)
        {
            playerObject = Instantiate(playerPrefab, playerParent);
        }

        playerNames.text = "";
    }

    private void Update()
    {
        // Rotar el modelo del jugador
        if (playerObject != null)
        {
            playerObject.transform.Rotate(0, 50 * Time.deltaTime, 0);
        }
    }

    private bool DetermineIfHost()
    {
        // Asegurarse de que el rol esté correctamente asignado en el GameConfigSO
        if (gameConfig.PlayerRole == "Host")
        {
            return true; // Es el Host
        }
        return false; // Es el Cliente
    }

    public void JoinPlayer(PlayerData player, EndPoint remoteEP = null)
    {
        if (!playerNamesList.ContainsKey(player.PlayerName))
        {
            playerNamesList[player.PlayerName] = remoteEP;
            playerNames.text += player.PlayerName + "\n";
        }
    }

    public void ReturnToStart()
    {
        SceneLoader.LoadStart();
    }

    public void GoToGame()
    {
        SceneLoader.LoadGameScene();
    }
}
