using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceToCam : MonoBehaviour
{
    private Camera fpsCamera;  // La c�mara FPS a la que el texto debe mirar.

    void Start()
    {
        // Busca la c�mara activa en la escena (puedes usar "FPS" si es necesario especificar por nombre).
        fpsCamera = Camera.main;  // Asumiendo que la c�mara principal es la que necesitamos.

        // Si no se encuentra la c�mara principal, busca espec�ficamente la c�mara FPS por su nombre.
        if (fpsCamera == null)
        {
            fpsCamera = GameObject.Find("FPS").GetComponent<Camera>();
        }

        if (fpsCamera == null)
        {
            Debug.LogError("No se encontr� la c�mara FPS.");
        }
    }

    void Update()
    {
        if (fpsCamera != null)
        {
            // El nombre siempre mira a la c�mara en el eje Y.
            Vector3 direction = fpsCamera.transform.position - transform.position;
            direction.y = 0; // Mant�n la rotaci�n solo en el eje Y para que no se voltee en el eje X o Z.
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
