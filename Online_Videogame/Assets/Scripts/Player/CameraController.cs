using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float lookSpeedX = 2f;  // Velocidad de rotaci�n horizontal
    public float lookSpeedY = 2f;  // Velocidad de rotaci�n vertical
    public float upperLookLimit = 80f;  // L�mite superior de visi�n
    public float lowerLookLimit = -80f; // L�mite inferior de visi�n

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Update()
    {
        // Obtener el movimiento del rat�n
        rotationX += Input.GetAxis("Mouse X") * lookSpeedX;
        rotationY -= Input.GetAxis("Mouse Y") * lookSpeedY;

        // Limitar la rotaci�n vertical
        rotationY = Mathf.Clamp(rotationY, lowerLookLimit, upperLookLimit);

        // Aplicar la rotaci�n de la c�mara
        transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);

        // Rotar el cuerpo en la direcci�n de la rotaci�n horizontal
        // Hacer que el jugador rote en el eje Y basado en la rotaci�n de la c�mara
        transform.parent.rotation = Quaternion.Euler(0f, rotationX, 0f);
    }
}
