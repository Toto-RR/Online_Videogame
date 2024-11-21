using UnityEngine;
using TMPro;

public class ConsoleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI consoleText;
    private string logOutput = "";
    private int maxLogLines = 9;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        AppendToConsole(logString);
    }

    private void AppendToConsole(string message)
    {
        logOutput += $"{message}\n";

        var lines = logOutput.Split('\n');
        if (lines.Length > maxLogLines)
        {
            logOutput = string.Join("\n", lines, lines.Length - maxLogLines, maxLogLines);
        }

        if (consoleText != null)
        {
            consoleText.text = logOutput;
        }
    }

    public void LogToConsole(string message)
    {
        AppendToConsole(message);
    }

    public void ClearConsole()
    {
        logOutput = "";
        if (consoleText != null)
        {
            consoleText.text = logOutput;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearConsole();
        }
        if (Input.GetKeyUp(KeyCode.M))
        {
            LogToConsole("Mensaje de prueba desde el código.");
        }
    }

    public void ToggleConsole()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
