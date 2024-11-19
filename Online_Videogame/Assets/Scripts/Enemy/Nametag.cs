using UnityEngine;
using TMPro;

public class Nametag : MonoBehaviour
{
    private TMP_Text playerName;
    private Camera fpsCamera;

    void Start()
    {
        // Asegúrate de que el TMP_Text esté asignado
        playerName = GetComponent<TMP_Text>();
        SetName();

        // Intentar obtener la cámara principal
        fpsCamera = Camera.main;

        if (fpsCamera == null)
        {
            // Si no se encuentra la cámara principal, intenta buscarla por nombre
            fpsCamera = GameObject.Find("FPS")?.GetComponent<Camera>();

            if (fpsCamera == null)
            {
                // Si no se encuentra la cámara por nombre, busca todas las cámaras en la escena
                Camera[] cameras = FindObjectsOfType<Camera>();
                if (cameras.Length > 0)
                {
                    fpsCamera = cameras[0]; // Selecciona la primera cámara que encuentre
                    Debug.Log("Cámara encontrada por tipo.");
                }
                else
                {
                    Debug.LogError("No se encontró ninguna cámara en la escena.");
                }
            }
            else
            {
                Debug.Log("Cámara encontrada por nombre.");
            }
        }
        else
        {
            Debug.Log("Cámara principal encontrada.");
        }

        if (fpsCamera == null)
        {
            Debug.LogError("No se pudo encontrar una cámara válida.");
        }
    }

    void Update()
    {
        // Si la cámara está asignada, hacer que el nombre siempre mire a la cámara
        if (fpsCamera != null)
        {
            Vector3 direction = fpsCamera.transform.position - transform.position;
            direction.y = 0; // Solo rota en el eje Y para evitar inclinación
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    // Establece el nombre basado en el objeto padre
    public void SetName()
    {
        if (transform.parent != null)
        {
            playerName.text = transform.parent.gameObject.name; // Obtiene el nombre del objeto padre
        }
        else
        {
            Debug.LogWarning("El objeto no tiene un padre.");
        }
    }
}
