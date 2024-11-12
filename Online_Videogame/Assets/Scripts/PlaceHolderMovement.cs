using UnityEngine;

public class PlaceholderMovement : MonoBehaviour
{
    // Esto es para almacenar la posici�n que recibimos de otro jugador (host o cliente)
    public Vector3 targetPosition;

    // Velocidad con la que el placeholder se mueve
    public float movementSpeed = 5f;

    void Update()
    {
        // Si hay una nueva posici�n, mueve el placeholder hacia esa posici�n
        if (targetPosition != null)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementSpeed);
        }
    }

    // M�todo para actualizar la posici�n del placeholder
    public void UpdatePosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
    }
}
