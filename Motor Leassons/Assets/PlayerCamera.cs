using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCamera : MonoBehaviour
{
    public Transform playerBody; // Referencia al cuerpo del jugador (donde rota en horizontal)
    public Transform playerCamera; // Cámara que rota en vertical
    public float mouseSensitivity = 2f;

    private float verticalRotation = 0f;



    void Update()
    {

            HandleMouseLook();
        
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotación horizontal del cuerpo
        playerBody.Rotate(Vector3.up * mouseX);

        // Rotación vertical de la cámara (limitada)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}
