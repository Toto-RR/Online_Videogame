using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public UDP_Server hostServerScript;      // Host script
    public UDP_Client clientScript;          // Client script
    public GameObject playerPrefab;          // Player Prefab
    public GameConfigSO gameConfig;          // GameConfigSO reference

    public GameObject clientRespawn;
    public GameObject serverRespawn;

    void Start()
    {
        // Check GameConfigSO
        if (gameConfig == null)
        {
            Debug.LogError("GameConfigSO is not assigned at GameSceneManager.");
            return;
        }

        bool isHost = DetermineIfHost();

        if (isHost)
        {
            // Si es Host
            hostServerScript.enabled = true;   // Enable host script
            clientScript.enabled = false;      // Disable client script

            gameConfig.SetRespawnPos(serverRespawn.transform.position);
            gameConfig.SetRespawnRot(serverRespawn.transform.rotation);

            Instantiate(playerPrefab, serverRespawn.transform.position, Quaternion.identity);
        }
        else
        {
            // Si es Cliente
            clientScript.enabled = true;      // Enable client script
            hostServerScript.enabled = false; // Disable host script

            gameConfig.SetRespawnPos(clientRespawn.transform.position);
            gameConfig.SetRespawnRot(clientRespawn.transform.rotation);

            Instantiate(playerPrefab, clientRespawn.transform.position, Quaternion.identity);
        }

        
        //playerPrefab.SetActive(true);
    }

    bool DetermineIfHost()
    {
        if (gameConfig.PlayerRole == "Host")
        {
            return true; // Is host
        }
        return false; // Is client
    }
}
