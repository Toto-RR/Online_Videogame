using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public static void LoadLobbyScene()
    {
        SceneManager.LoadScene("Lobby");
    }
}
