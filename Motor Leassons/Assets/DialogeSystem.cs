using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{
    public List<GameObject> objects; // Lista de objetos

    private int currentIndex = 0; // �ndice del objeto actual

    // Referencias a los scripts de movimiento y raycast
    public PlayerMove playerMoveScript;
    public RaycastSystem raycastScript;

    private void Start()
    {
        // Asegurarse de que solo el primer objeto est� activo al inicio
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(i == 0); // Activar solo el primero
            }
        }

        // Congelar los sistemas al inicio del di�logo
        if (playerMoveScript != null)
        {
            playerMoveScript.isMovementBlocked = true;
        }

        if (raycastScript != null)
        {
            raycastScript.isSystemFrozen = true;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentIndex < objects.Count)
            {
                if (objects[currentIndex] != null)
                {
                    objects[currentIndex].SetActive(false); // Desactivar el objeto actual
                    Debug.Log($"Objeto {currentIndex + 1} desactivado");
                }

                currentIndex++; // Pasar al siguiente objeto


                if (currentIndex == 5)
                {
                    Debug.Log("Se han le�do 5 di�logos.");
                }

                if (currentIndex < objects.Count && objects[currentIndex] != null)
                {
                    objects[currentIndex].SetActive(true); // Activar el siguiente objeto
                    Debug.Log($"Objeto {currentIndex + 1} activado");
                }
                else if (currentIndex >= objects.Count)
                {
                    Debug.Log("No m�s objetos para manejar");

                    // Descongelar los sistemas cuando termine el di�logo
                    if (playerMoveScript != null)
                    {
                        playerMoveScript.isMovementBlocked = false;
                    }

                    if (raycastScript != null)
                    {
                        raycastScript.isSystemFrozen = false;
                    }
                }
            }
        }
    }
}
