using UnityEngine;
using UnityEngine.UI;

public class PauseMenuSettings : MonoBehaviour
{
    [Header("Player Settings")]
    public FPSController playerController; // Referencia al controlador de movimiento
    public Camera playerCamera; // Referencia a la cámara del jugador

    [Header("Sliders")]
    public Slider sensitivitySlider;
    public Slider fovSlider;

    private void Start()
    {
        // Inicializar sliders con los valores actuales
        sensitivitySlider.value = playerController.lookSpeed;
        fovSlider.value = playerCamera.fieldOfView;

        // Añadir listeners para actualizar valores en tiempo real
        sensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
        fovSlider.onValueChanged.AddListener(UpdateFOV);
    }

    private void UpdateSensitivity(float value)
    {
        playerController.lookSpeed = value;
    }

    private void UpdateFOV(float value)
    {
        playerController.SetFOV(value);
    }

}
