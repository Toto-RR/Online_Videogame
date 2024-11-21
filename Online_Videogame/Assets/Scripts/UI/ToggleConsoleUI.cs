using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleConsoleUI : MonoBehaviour
{
    public ConsoleUI consoleUI;

    // Start is called before the first frame update
    void Start()
    {
        if (consoleUI == null)
            consoleUI = GetComponent<ConsoleUI>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            consoleUI.ToggleConsole();
        }
    }

    public void ToggleConsole()
    {
        consoleUI.ToggleConsole();
    }
}
