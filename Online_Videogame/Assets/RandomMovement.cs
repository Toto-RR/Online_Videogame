using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    public float speed = 5f; // Velocidad de movimiento
    public float range = 10f; // Rango de movimiento lateral

    private float startPositionX; // Posición inicial en X
    private bool movingRight = true; // Dirección inicial

    void Start()
    {
        // Guardamos la posición inicial en X
        startPositionX = transform.position.x;
    }

    void Update()
    {
        // Verificar si estamos en el rango permitido
        if (movingRight)
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime);

            // Si se supera el rango, cambia de dirección
            if (transform.position.x >= startPositionX + range)
                movingRight = false;
        }
        else
        {
            transform.Translate(Vector3.left * speed * Time.deltaTime);

            // Si se supera el rango, cambia de dirección
            if (transform.position.x <= startPositionX - range)
                movingRight = true;
        }
    }
}
