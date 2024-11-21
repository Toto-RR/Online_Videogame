using UnityEngine;
using TMPro;

public class ConsoleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI consoleText; // Asocia el objeto TextMeshPro desde el editor
    private string logOutput = ""; // Almacenará los logs
    private int maxLogLines = 100; // Máximo de líneas

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
        // Agregar el mensaje al texto de la consola
        AppendToConsole(logString);
    }

    private void AppendToConsole(string message)
    {
        logOutput += $"{message}\n";

        // Limitar el número de líneas
        var lines = logOutput.Split('\n');
        if (lines.Length > maxLogLines)
        {
            logOutput = string.Join("\n", lines, lines.Length - maxLogLines, maxLogLines);
        }

        // Actualizar el texto en la UI
        if (consoleText != null)
        {
            consoleText.text = logOutput;
        }
    }

    // Método público para registrar mensajes adicionales
    public void LogToConsole(string message)
    {
        AppendToConsole(message); // Añadir mensaje a la consola
    }

    // Limpiar la consola si es necesario
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
        // Detectar si se presionan teclas específicas
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearConsole(); // Limpia la consola
        }
        if (Input.GetKeyUp(KeyCode.M))
        {
            LogToConsole("Example message.");
        }
    }

    public void ToggleConsole()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
