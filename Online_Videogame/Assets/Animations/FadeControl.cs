using System.Collections;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public CanvasGroup mainMenuCanvas; // Canvas inicial
    public CanvasGroup hostCanvas;    // Canvas de host
    public CanvasGroup joinCanvas;    // (Opcional) Otro Canvas

    private CanvasGroup currentCanvas; // El Canvas actualmente activo

    private void Start()
    {
        
    }

    public void SwitchCanvas(CanvasGroup canvasToGo)
    {

    }
}
