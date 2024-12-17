using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public UDP_Server hostServerScript;      // Script para el host
    public UDP_Client clientScript;          // Script para el cliente
    public GameObject playerPrefab;          // Player Prefab
    public GameConfigSO gameConfig;          // Referencia al GameConfigSO

    public GameObject clientRespawn;
    public GameObject serverRespawn;

    void Start()
    {
        // Asegurarse de que el GameConfigSO esté asignado
        if (gameConfig == null)
        {
            Debug.LogError("GameConfigSO no está asignado en el GameSceneManager.");
            return;
        }

        bool isHost = DetermineIfHost(); // Función para determinar el rol (Host o Cliente)

        if (isHost)
        {
            // Si es Host
            hostServerScript.enabled = true;   // Activar el servidor (host)
            clientScript.enabled = false;      // Desactivar el script del cliente

            gameConfig.SetRespawnPos(serverRespawn.transform.position);
            gameConfig.SetRespawnRot(serverRespawn.transform.rotation);

            Instantiate(playerPrefab, serverRespawn.transform.position, Quaternion.identity);
        }
        else
        {
            // Si es Cliente
            clientScript.enabled = true;      // Activar el script del cliente
            hostServerScript.enabled = false; // Desactivar el script del host

            gameConfig.SetRespawnPos(clientRespawn.transform.position);
            gameConfig.SetRespawnRot(clientRespawn.transform.rotation);

            Instantiate(playerPrefab, clientRespawn.transform.position, Quaternion.identity);
        }

        
        //playerPrefab.SetActive(true);
    }

    bool DetermineIfHost()
    {
        // Asegurarse de que el rol esté correctamente asignado en el GameConfigSO
        if (gameConfig.PlayerRole == "Host")
        {
            return true; // Es el Host
        }
        return false; // Es el Cliente
    }
}
