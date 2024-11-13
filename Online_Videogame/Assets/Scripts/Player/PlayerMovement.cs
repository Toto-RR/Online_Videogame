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

    public UDP_Client udpClient;
    private Vector3 lastClientPosition;
    private Quaternion lastClientRotation;

    public UDP_Server udpServer;
    private Vector3 lastServerPosition;
    private Quaternion lastServerRotation;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;


    CharacterController characterController;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        lastClientPosition = transform.position;
        lastClientRotation = transform.rotation;

        lastServerPosition = transform.position;
        lastServerRotation = transform.rotation;
    }

    void Update()
    {

        #region Handles Movment
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        //Client
        if (udpClient.enabled == true && //Si el cliente est� activado y su posici�n o rotaci�n cambian, entonces actualiza
            ((transform.position != lastClientPosition) || (transform.rotation != lastClientRotation)))
        {
            //Actualiza la �ltima posicion y rotaci�n del player
            lastClientPosition = transform.position;
            lastClientRotation = transform.rotation;

            //Manda la posici�n y rotaci�n al Cliente
            udpClient.SendMovementAndRotation(lastClientPosition, lastClientRotation);
        }

        //Host
        if (udpServer.enabled == true && //Si el servidor est� activado y su rotacion o posici�n cambian, entonces actualiza
            ((transform.position != lastServerPosition) || (transform.rotation != lastServerRotation)))
        {
            //Actualiza la �ltima posicion y rotaci�n del player
            lastServerPosition = transform.position;
            lastServerRotation = transform.rotation;
            
            //Manda la posici�n y rotaci�n al Server (para que la mande a los clientes)
            //udpServer.BroadcastGameState();
        }
        #endregion

        #region Handles Jumping
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        #endregion

        #region Handles Rotation
        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        #endregion
    }
}