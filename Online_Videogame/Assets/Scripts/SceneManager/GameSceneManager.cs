using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public UDP_Server hostServerScript;      // Script para el host
    public UDP_Client clientScript;          // Script para el cliente
    public GameObject playerPrefab;            // Player Prefab
    public GameConfigSO gameConfig;          // Referencia al GameConfigSO

    void Start()
    {
        // Asegurarse de que el GameConfigSO est� asignado
        if (gameConfig == null)
        {
            Debug.LogError("GameConfigSO no est� asignado en el GameSceneManager.");
            return;
        }

        bool isHost = DetermineIfHost(); // Funci�n para determinar el rol (Host o Cliente)

        if (isHost)
        {
            // Si es Host
            hostServerScript.enabled = true;   // Activar el servidor (host)
            clientScript.enabled = false;      // Desactivar el script del cliente
        }
        else
        {
            // Si es Cliente
            clientScript.enabled = true;      // Activar el script del cliente
            hostServerScript.enabled = false; // Desactivar el script del host
        }

        playerPrefab.SetActive(true);
    }

    bool DetermineIfHost()
    {
        // Asegurarse de que el rol est� correctamente asignado en el GameConfigSO
        if (gameConfig.PlayerRole == "Host")
        {
            return true; // Es el Host
        }
        return false; // Es el Cliente
    }
}
