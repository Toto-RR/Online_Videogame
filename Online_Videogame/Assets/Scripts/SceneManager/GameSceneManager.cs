using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public UDP_Server hostServerScript;      // Script para el host
    public UDP_Client clientScript;          // Script para el cliente
    public GameObject hostPlayer;            // Prefab del jugador del Host
    public GameObject clientPlayer;          // Prefab del jugador del Cliente
    public GameConfigSO gameConfig;          // Referencia al GameConfigSO

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
            hostPlayer.SetActive(true);        // Activar el jugador del host
            clientPlayer.SetActive(false);     // Desactivar el jugador del cliente
        }
        else
        {
            // Si es Cliente
            clientScript.enabled = true;      // Activar el script del cliente
            hostServerScript.enabled = false; // Desactivar el script del host
            clientPlayer.SetActive(true);     // Activar el jugador del cliente
            hostPlayer.SetActive(false);      // Desactivar el jugador del host
        }
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
