using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceToCam : MonoBehaviour
{
    private Camera fpsCamera;  // La cámara FPS a la que el texto debe mirar.

    void Start()
    {
        // Busca la cámara activa en la escena (puedes usar "FPS" si es necesario especificar por nombre).
        fpsCamera = Camera.main;  // Asumiendo que la cámara principal es la que necesitamos.

        // Si no se encuentra la cámara principal, busca específicamente la cámara FPS por su nombre.
        if (fpsCamera == null)
        {
            fpsCamera = GameObject.Find("FPS").GetComponent<Camera>();
        }

        if (fpsCamera == null)
        {
            Debug.LogError("No se encontró la cámara FPS.");
        }
    }

    void Update()
    {
        if (fpsCamera != null)
        {
            // El nombre siempre mira a la cámara en el eje Y.
            Vector3 direction = fpsCamera.transform.position - transform.position;
            direction.y = 0; // Mantén la rotación solo en el eje Y para que no se voltee en el eje X o Z.
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
