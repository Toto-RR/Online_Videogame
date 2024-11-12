using UnityEngine;
using UnityEngine.SceneManagement;  // Para cargar escenas
using UnityEngine.UI;  // Para los botones UI

public class GameManager : MonoBehaviour
{
    public Button hostButton;
    public Button joinButton;

    private string selectedRole;

    void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        joinButton.onClick.AddListener(JoinServer);
    }

    // Al hacer clic en "Host Game"
    void StartHost()
    {
        selectedRole = "Host";  // El rol del jugador será "Host"
        PlayerPrefs.SetString("PlayerRole", selectedRole);  // Guardamos el rol en PlayerPrefs
        SceneManager.LoadScene("GameScene");  // Cargar la escena del juego
    }

    // Al hacer clic en "Join Game"
    void JoinServer()
    {
        selectedRole = "Client";  // El rol del jugador será "Client"
        PlayerPrefs.SetString("PlayerRole", selectedRole);  // Guardamos el rol en PlayerPrefs
        SceneManager.LoadScene("GameScene");  // Cargar la escena del juego
    }
}
