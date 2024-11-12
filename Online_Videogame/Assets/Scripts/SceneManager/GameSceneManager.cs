using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public GameObject hostPlayer;  // Host
    public GameObject clientPlayer;  // Cliente

    private UDP_Server udpServer = null;
    private UDP_Client udpClient = null;

    void Start()
    {
        string playerRole = PlayerPrefs.GetString("PlayerRole");

        if (playerRole == "Host")
        {
            // El jugador es el Host, activamos su objeto
            hostPlayer.SetActive(true);
            if (clientPlayer.activeInHierarchy) clientPlayer.SetActive(false);

            // Asignamos el servidor del player a la variable
            udpServer = hostPlayer.GetComponent<UDP_Server>();

            // Si no lo encuentra, da error
            if (udpServer == null) Debug.LogError("UDP Server not founded");

            // Si el componente estaba desactivado (es la idea) activalo y inicia servidor
            if (udpServer.isActiveAndEnabled)
            {
                udpServer.enabled = true;
                udpServer.StartServer();
            }
            // Si el componente ya estaba activado, simplemente inici servidor
            else
            {
                udpServer.StartServer();
            }
        }
        else if (playerRole == "Client")
        {
            // El jugador es el Cliente, activamos su objeto
            if (hostPlayer.activeInHierarchy) hostPlayer.SetActive(false);
            clientPlayer.SetActive(true);

            // Asignamos el cliente del player a la variable
            udpClient = clientPlayer.GetComponent<UDP_Client>();

            // Si no lo encuentra da error
            if (udpClient == null) Debug.LogError("UDP Client not found");

            // Si el componente estaba desactivado (es la idea) activalo y inicia cliente
            if (udpClient.isActiveAndEnabled)
            {
                udpClient.enabled = true;
                udpClient.ConnectToServer("127.0.0.1", 9050);
            }
            // Si el componente ya estaba activado, simplemente inicia cliente
            else
            {
                udpClient.ConnectToServer("127.0.0.1", 9050);
            }

        }
    }
}
