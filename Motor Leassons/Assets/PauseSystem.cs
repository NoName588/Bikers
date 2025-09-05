using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseSystem : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject mainUI;   // UI principal (se desactiva al pausar)
    public GameObject pauseUI;  // UI de pausa (se activa al pausar)

    [Header("References")]
    public RaycastSystem raycastSystem; // 🔹 Asigna aquí el RaycastSystem

    [Header("Game State")]
    public bool isPaused = false;

    [Header("Animator")]
    public Animator Fadeanimator;

    [Header("Timing")]
    public float waitBeforeScene = 2f; // Tiempo de espera antes de cambiar de escena

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;

        if (mainUI != null) mainUI.SetActive(false);
        if (pauseUI != null) pauseUI.SetActive(true);

        if (raycastSystem != null)
            raycastSystem.isSystemFrozen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Juego pausado.");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (mainUI != null) mainUI.SetActive(true);
        if (pauseUI != null) pauseUI.SetActive(false);

        if (raycastSystem != null)
            raycastSystem.isSystemFrozen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Juego reanudado.");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        if (raycastSystem != null)
            raycastSystem.isSystemFrozen = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu(string menuSceneName)
    {
        StartCoroutine(GoToMenuCoroutine(menuSceneName));
    }

    private IEnumerator GoToMenuCoroutine(string menuSceneName)
    {
        // 🔹 Activar mouse y congelar RaycastSystem inmediatamente
        if (raycastSystem != null)
            raycastSystem.isSystemFrozen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 🔹 Restaurar tiempo de juego por si estaba pausado
        Time.timeScale = 1f;

        if (Fadeanimator != null)
            Fadeanimator.SetTrigger("TrFade");
        // 🔹 Espera antes de cambiar de escena
        yield return new WaitForSeconds(waitBeforeScene);

        // 🔹 Cargar la escena de menú
        SceneManager.LoadScene(menuSceneName);
    }
}


