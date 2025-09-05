using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [Header("Animators")]
    public Animator Fade_animator;

    [Header("Tiempo entre animaciones y escena")]
    public float waitBetweenAnims = 2f; // segundos entre cada animación
    public float waitBeforeScene = 2f;  // segundos antes de cambiar de escena

    // Llamado desde el botón
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        if (Fade_animator != null)
            Fade_animator.SetTrigger("TrFade");

        // Espera antes de cambiar de escena
        yield return new WaitForSeconds(waitBeforeScene);

        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Para salir en el editor
#else
        Application.Quit(); // Para salir en el build
#endif
    }
}

