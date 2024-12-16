using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class IdentifyRole : MonoBehaviour
{
    public UDP_Server hostServerScript;
    public UDP_Client clientScript;
    public GameConfigSO gameConfig;

    public bool isHost = false;

    // Start is called before the first frame update
    void Start()
    {
        // Asegurarse de que el GameConfigSO esté asignado
        if (gameConfig == null)
        {
            Debug.LogError("GameConfigSO no está asignado en el GameSceneManager.");
            return;
        }

        isHost = DetermineIfHost(); // Función para determinar el rol (Host o Cliente)

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
