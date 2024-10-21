using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneChanger : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI errorMessageText;

    void Start()
    {
        errorMessageText.text = "";
    }

    public void ChangeScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            UserData.username = inputField.text;
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            errorMessageText.text = "Please, enter your username";
        }
    }

    public void ClearErrorMessage()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            errorMessageText.text = "";
        }
    }
}
