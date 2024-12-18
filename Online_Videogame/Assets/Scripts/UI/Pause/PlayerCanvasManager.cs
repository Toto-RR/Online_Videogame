using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCanvasManager : MonoBehaviour
{
    public Canvas canvasHud;
    public Canvas canvasPause;

    private void Start()
    {
        //Start with HUD
        SetCanvasHUD();
    }

    public void SetCanvasHUD()
    {
        if(canvasHud != null)
        {
            canvasHud.gameObject.SetActive(true);
            canvasPause.gameObject.SetActive(false);
        }
    }

    public void SetCanvasPause()
    {
        if(canvasPause != null)
        {
            canvasPause.gameObject.SetActive(true);
            canvasHud.gameObject.SetActive(false);
        }
    }

    public bool IsPaused()
    {
        if (canvasPause != null && canvasPause.gameObject.activeInHierarchy) return true;
        else return false;
    }
}
