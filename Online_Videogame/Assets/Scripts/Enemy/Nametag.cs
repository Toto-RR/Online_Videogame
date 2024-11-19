using UnityEngine;
using TMPro;

public class Nametag : MonoBehaviour
{
    private TMP_Text playerName;
    private Camera fpsCamera;

    void Start()
    {
        // Aseg�rate de que el TMP_Text est� asignado
        playerName = GetComponent<TMP_Text>();
        SetName();

        // Intentar obtener la c�mara principal
        fpsCamera = Camera.main;

        if (fpsCamera == null)
        {
            // Si no se encuentra la c�mara principal, intenta buscarla por nombre
            fpsCamera = GameObject.Find("FPS")?.GetComponent<Camera>();

            if (fpsCamera == null)
            {
                // Si no se encuentra la c�mara por nombre, busca todas las c�maras en la escena
                Camera[] cameras = FindObjectsOfType<Camera>();
                if (cameras.Length > 0)
                {
                    fpsCamera = cameras[0]; // Selecciona la primera c�mara que encuentre
                    Debug.Log("C�mara encontrada por tipo.");
                }
                else
                {
                    Debug.LogError("No se encontr� ninguna c�mara en la escena.");
                }
            }
            else
            {
                Debug.Log("C�mara encontrada por nombre.");
            }
        }
        else
        {
            Debug.Log("C�mara principal encontrada.");
        }

        if (fpsCamera == null)
        {
            Debug.LogError("No se pudo encontrar una c�mara v�lida.");
        }
    }

    void Update()
    {
        // Si la c�mara est� asignada, hacer que el nombre siempre mire a la c�mara
        if (fpsCamera != null)
        {
            Vector3 direction = fpsCamera.transform.position - transform.position;
            direction.y = 0; // Solo rota en el eje Y para evitar inclinaci�n
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
