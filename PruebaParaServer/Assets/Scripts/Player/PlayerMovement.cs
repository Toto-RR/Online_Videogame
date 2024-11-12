using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;

    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;

    CharacterController characterController;

    private UDP_Client udpClient;
    private Vector3 lastPosition;

    private bool jumpSent = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        udpClient = FindObjectOfType<UDP_Client>();
    }

    void Update()
    {
        // Movimiento del jugador
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float speed = isRunning ? runSpeed : walkSpeed;
        Vector3 movement = (forward * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal")) * speed;

        float movementDirectionY = moveDirection.y;
        moveDirection = movement;

        // Manejo de salto
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded && !jumpSent)
        {
            moveDirection.y = jumpPower;
            udpClient?.SendMessage("JUMP");
            jumpSent = true; // Marcar que el salto ha sido enviado
        }
        else if (characterController.isGrounded)
        {
            jumpSent = false; // Restablecer cuando el jugador esté en el suelo
        }

        // Aplicar gravedad si no está en el suelo
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Movimiento y rotación
        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        // Solo enviar la posición si ha cambiado de forma significativa
        if (Vector3.Distance(transform.position, lastPosition) > 0.1f)
        {
            udpClient?.SendMessage("MOVE");
            lastPosition = transform.position; // Actualizar la última posición enviada
        }
    }
}
