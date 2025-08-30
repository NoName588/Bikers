using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{
    [Header("Diálogo normal")]
    public List<GameObject> objects; // Lista de diálogos normales
    private int currentIndex = 0;    // Índice del objeto actual

    [Header("Preguntas")]
    public bool isQuestion = false;             // 🔹 Indica si este diálogo es una pregunta
    public List<GameObject> correctObjects;     // Objetos si la respuesta es correcta
    public List<GameObject> incorrectObjects;   // Objetos si la respuesta es incorrecta
    private int questionIndex = 0;              // Índice para recorrer la lista de la respuesta
    private List<GameObject> activeQuestionList; // Lista activa según la elección del jugador

    [Header("UI Extra")]
    public GameObject objectToDisableOnAnswer; // 🔹 Objeto que se desactiva al responder

    [Header("Finalización")]
    public bool inactivarAlFinal = false;       // 🔹 Si se inactiva al final
    public GameObject objectToDisableAtEnd;     // 🔹 Objeto que se desactiva al acabar el diálogo/pregunta

    public bool activarAlFinal = false;         // 🔹 Si se activa al final
    public GameObject objectToEnableAtEnd;      // 🔹 Objeto que se activa al acabar el diálogo/pregunta

    // Referencias a los scripts de movimiento y raycast
    public PlayerMove playerMoveScript;
    public RaycastSystem raycastScript;

    private void Start()
    {
        // Inicializar diálogos normales
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(i == 0); // Solo activar el primero
            }
        }

        // Congelar al inicio
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
        if (!isQuestion) // 🔹 Caso normal de diálogos
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (currentIndex < objects.Count)
                {
                    if (objects[currentIndex] != null)
                    {
                        objects[currentIndex].SetActive(false);
                        Debug.Log($"Objeto {currentIndex + 1} desactivado");
                    }

                    currentIndex++;

                    if (currentIndex == 5)
                    {
                        Debug.Log("Se han leído 5 diálogos.");
                    }

                    if (currentIndex < objects.Count && objects[currentIndex] != null)
                    {
                        objects[currentIndex].SetActive(true);
                        Debug.Log($"Objeto {currentIndex + 1} activado");
                    }
                    else if (currentIndex >= objects.Count)
                    {
                        Debug.Log("No más objetos para manejar");

                        HandleEndOfDialogue();
                    }
                }
            }
        }
        else // 🔹 Caso pregunta con respuestas
        {
            if (Input.GetKeyDown(KeyCode.Return) && activeQuestionList != null)
            {
                if (questionIndex < activeQuestionList.Count)
                {
                    if (activeQuestionList[questionIndex] != null)
                    {
                        activeQuestionList[questionIndex].SetActive(false);
                        Debug.Log($"Respuesta {questionIndex + 1} desactivada");
                    }

                    questionIndex++;

                    if (questionIndex < activeQuestionList.Count && activeQuestionList[questionIndex] != null)
                    {
                        activeQuestionList[questionIndex].SetActive(true);
                        Debug.Log($"Respuesta {questionIndex + 1} activada");
                    }
                    else if (questionIndex >= activeQuestionList.Count)
                    {
                        Debug.Log("No más respuestas para manejar");

                        HandleEndOfDialogue();
                    }
                }
            }
        }
    }

    // 🔹 Método a llamar desde el botón de respuesta correcta
    public void ChooseCorrect()
    {
        if (isQuestion && correctObjects.Count > 0)
        {
            StartQuestion(correctObjects);
            Debug.Log("Respuesta CORRECTA elegida");

            DisableExtraObject();
        }
    }

    // 🔹 Método a llamar desde el botón de respuesta incorrecta
    public void ChooseIncorrect()
    {
        if (isQuestion && incorrectObjects.Count > 0)
        {
            StartQuestion(incorrectObjects);
            Debug.Log("Respuesta INCORRECTA elegida");

            DisableExtraObject();
        }
    }

    private void StartQuestion(List<GameObject> chosenList)
    {
        // Desactivar diálogos normales si estaban activos
        foreach (var obj in objects)
        {
            if (obj != null) obj.SetActive(false);
        }

        activeQuestionList = chosenList;
        questionIndex = 0;

        // Activar el primer objeto de la lista
        if (activeQuestionList[questionIndex] != null)
        {
            activeQuestionList[questionIndex].SetActive(true);
        }
    }

    private void DisableExtraObject()
    {
        if (objectToDisableOnAnswer != null)
        {
            objectToDisableOnAnswer.SetActive(false);
            Debug.Log("Objeto extra desactivado al responder.");
        }
    }

    private void HandleEndOfDialogue()
    {
        // Descongelar sistemas
        UnfreezeSystems();

        // 🔹 Inactivar objeto final si está configurado
        if (inactivarAlFinal && objectToDisableAtEnd != null)
        {
            objectToDisableAtEnd.SetActive(false);
            Debug.Log("Objeto final inactivado al terminar el diálogo.");
        }

        // 🔹 Activar objeto final si está configurado
        if (activarAlFinal && objectToEnableAtEnd != null)
        {
            objectToEnableAtEnd.SetActive(true);
            Debug.Log("Objeto final ACTIVADO al terminar el diálogo.");
        }
    }

    private void UnfreezeSystems()
    {
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
