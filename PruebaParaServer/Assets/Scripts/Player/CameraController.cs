using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float lookSpeedX = 2f;  // Velocidad de rotación horizontal
    public float lookSpeedY = 2f;  // Velocidad de rotación vertical
    public float upperLookLimit = 80f;  // Límite superior de visión
    public float lowerLookLimit = -80f; // Límite inferior de visión

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Update()
    {
        // Obtener el movimiento del ratón
        rotationX += Input.GetAxis("Mouse X") * lookSpeedX;
        rotationY -= Input.GetAxis("Mouse Y") * lookSpeedY;

        // Limitar la rotación vertical
        rotationY = Mathf.Clamp(rotationY, lowerLookLimit, upperLookLimit);

        // Aplicar la rotación de la cámara
        transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);

        // Rotar el cuerpo en la dirección de la rotación horizontal
        // Hacer que el jugador rote en el eje Y basado en la rotación de la cámara
        transform.parent.rotation = Quaternion.Euler(0f, rotationX, 0f);
    }
}
