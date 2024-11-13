using UnityEngine;
using UnityEngine.SceneManagement;  // Para cargar escenas
using UnityEngine.UI;  // Para los botones UI

public class GameManager : MonoBehaviour
{
    private string selectedRole;

    // Al hacer clic en "Host Game"
    public void StartHost()
    {
        selectedRole = "Host";  // El rol del jugador será "Host"
        PlayerPrefs.SetString("PlayerRole", selectedRole);  // Guardamos el rol en PlayerPrefs
        SceneManager.LoadScene("GameScene");  // Cargar la escena del juego
    }

    // Al hacer clic en "Join Game"
    public void JoinServer()
    {
        selectedRole = "Client";  // El rol del jugador será "Client"
        PlayerPrefs.SetString("PlayerRole", selectedRole);  // Guardamos el rol en PlayerPrefs
        SceneManager.LoadScene("GameScene");  // Cargar la escena del juego
    }
}
