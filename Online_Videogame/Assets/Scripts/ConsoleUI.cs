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
        //Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Debug para verificar si se captura el evento
        Debug.Log($"HandleLog capturado: {logString}");

        logOutput += $"{logString}\n";
        var lines = logOutput.Split('\n');
        if (lines.Length > maxLogLines)
        {
            logOutput = string.Join("\n", lines, lines.Length - maxLogLines, maxLogLines);
        }
        consoleText.text = logOutput;
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

        // Detecta si la tecla 'C' ha sido presionada para limpiar la consola
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearConsole(); // Limpia la consola
        }
        if (Input.GetKeyUp(KeyCode.M))
        {
            Debug.Log("Mensaje de prueba");
        }
    }
}
