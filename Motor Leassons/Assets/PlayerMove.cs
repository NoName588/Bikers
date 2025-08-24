using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public float sprintSpeed = 10f;
    public float mouseSensitivity = 2f;

    public Transform playerCamera;
    public Rigidbody rb;

    private float verticalRotation = 0f;
    private Vector3 moveDirection;
    private float currentSpeed;

    public bool isMovementBlocked = false;
    public bool isWASDBlocked = false;

    // --- Agachado ---
    public float crouchHeight = 0.5f; // Altura de la cámara agachado
    public float standHeight = 1f;    // Altura normal de la cámara
    public float crouchSpeed = 5f;    // Velocidad de interpolación
    private bool isCrouching = false;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        // Bloquear rotaciones físicas del rigidbody
        rb.freezeRotation = true;
    }

    void Start()
    {
        currentSpeed = speed;
    }

    void Update()
    {
        if (!isMovementBlocked)
        {
            if (!isWASDBlocked)
            {
                HandleMovement();
                HandleSprint();
                HandleCrouch();
            }

            HandleMouseLook();
        }
    }

    void FixedUpdate()
    {
        if (!isMovementBlocked && !isWASDBlocked)
        {
            MovePlayer();
        }
    }

    // Movimiento con WASD
    private void HandleMovement()
    {
        float mHorizontal = Input.GetAxis("Horizontal");
        float mVertical = Input.GetAxis("Vertical");

        moveDirection = (transform.forward * mVertical + transform.right * mHorizontal).normalized;
    }

    // Cámara con el mouse
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    // Sprint
    private void HandleSprint()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            currentSpeed = sprintSpeed;
        else
            currentSpeed = speed;
    }

    // Mover jugador con físicas
    private void MovePlayer()
    {
        rb.MovePosition(rb.position + moveDirection * currentSpeed * Time.fixedDeltaTime);
    }

    // --- Agacharse ---
    private void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }

        // Transición suave de altura de cámara
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        Vector3 newLocalPos = new Vector3(playerCamera.localPosition.x, targetHeight, playerCamera.localPosition.z);
        playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, newLocalPos, Time.deltaTime * crouchSpeed);
    }
}


