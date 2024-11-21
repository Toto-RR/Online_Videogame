using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float dashSpeed = 20f; // Dash Speed
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float dashDuration = 0.2f; // Dash duration in seconds
    public float dashCooldown = 1f; // Waiting time between dashes
    public int energyCostPerDash = 1; // Energy required for a dash

    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    public float dashFOV = 90f; // FOV during dash
    public float normalFOV = 60f; // Normal FOV
    public float fovTransitionSpeed = 10f; // FOV transition speed

    private Vector3 moveDirection = Vector3.zero;
    private Vector3 dashDirection = Vector3.zero;
    private float rotationX = 0;

    public bool canMove = true;

    private CharacterController characterController;

    private bool isDashing = false;
    private float dashTimeRemaining;
    private float lastDashTime;

    private Vector3 lastPos;
    private Quaternion lastRot;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerCamera.fieldOfView = normalFOV;

        lastPos = transform.position;
        lastRot = transform.rotation;
    }

    public void HandleMovement()
    {
        #region Handles Movement
        if (!isDashing)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);

            // Normal movement
            float curSpeedX = canMove ? walkSpeed * Input.GetAxis("Vertical") : 0;
            float curSpeedY = canMove ? walkSpeed * Input.GetAxis("Horizontal") : 0;
            float movementDirectionY = moveDirection.y;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

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

            #region Start Dash
            if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time > lastDashTime + dashCooldown)
            {
                TryStartDash();
            }
            #endregion
        }
        else
        {
            // Manejar el movimiento durante el dash
            dashTimeRemaining -= Time.deltaTime;
            if (dashTimeRemaining > 0)
            {
                moveDirection = dashDirection * dashSpeed;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, dashFOV, fovTransitionSpeed * Time.deltaTime); // Transición al FOV del dash
            }
            else
            {
                EndDash();
            }
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

        SendMovement();
    }

    private void TryStartDash()
    {
        // Verificar si el jugador tiene suficiente energía
        if (Player.Instance.GetEnergy() >= energyCostPerDash)
        {
            StartDash();
        }
        else
        {
            Debug.Log("Not enough energy to dash!");
        }
    }

    private void StartDash()
    {
        // Reduce player energy
        Player.Instance.currentEnergy -= energyCostPerDash;

        isDashing = true;
        dashTimeRemaining = dashDuration;
        lastDashTime = Time.time;

        // Determine dash direction
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float inputVertical = Input.GetAxisRaw("Vertical");
        float inputHorizontal = Input.GetAxisRaw("Horizontal");

        dashDirection = (forward * inputVertical + right * inputHorizontal).normalized;

        if (dashDirection == Vector3.zero)
        {
            dashDirection = forward;
        }

        Debug.Log("Dash started in direction: " + dashDirection + " Remaining energy: " + Player.Instance.GetEnergy());
    }

    private void EndDash()
    {
        isDashing = false;
        StartCoroutine(RestoreFOV());
    }

    private IEnumerator RestoreFOV()
    {
        while (Mathf.Abs(playerCamera.fieldOfView - normalFOV) > 0.1f)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, normalFOV, fovTransitionSpeed * Time.deltaTime);
            yield return null;
        }
        playerCamera.fieldOfView = normalFOV;
    }

    public void SendMovement()
    {
        if(lastPos != transform.position || lastRot != transform.rotation)
        {
            lastPos = transform.position;
            lastRot = transform.rotation;

            PlayerSync.Instance.SendPositionUpdate(transform.position, transform.rotation);
        }
    }
}
