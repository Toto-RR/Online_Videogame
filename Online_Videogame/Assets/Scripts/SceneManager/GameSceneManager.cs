using Unity.VisualScripting;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public UDP_Server hostServerScript;
    public UDP_Client clientScript;
    public GameObject hostPlayer;
    public GameObject clientPlayer;

    void Start()
    {
        bool isHost = DetermineIfHost(); // Función para determinar el rol (puedes usar un botón o una variable)

        if (isHost)
        {
            // Activar el servidor (host) y su jugador
            hostServerScript.enabled = true;
            clientScript.enabled = false;
            hostPlayer.SetActive(true);
            clientPlayer.SetActive(false);
        }
        else
        {
            // Activar el cliente y su jugador
            clientScript.enabled = true;
            hostServerScript.enabled = false;
            clientPlayer.SetActive(true);
            hostPlayer.SetActive(false);
        }
    }

    bool DetermineIfHost()
    {
        string playerRole = PlayerPrefs.GetString("PlayerRole");

        if (playerRole == "Host") return true;
        return false;
    }
}
