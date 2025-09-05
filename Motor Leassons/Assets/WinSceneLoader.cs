using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class WinSceneLoader : MonoBehaviour
{
    [Header("Referencia al RaycastSystem")]
    public RaycastSystem raycastSystem; // Asigna aquí el objeto con el script RaycastSystem

    [Header("Nombre de la escena a cargar")]
    public string sceneName = "Carretera";

    [Header("Animators")]
    public Animator M_animator;
    public Animator W_animator;
    public Animator Fade_animator;

    [Header("Tiempo entre animaciones y escena")]
    public float waitBetweenAnims = 2f; // segundos entre cada animación
    public float waitBeforeScene = 2f;  // segundos antes de cambiar de escena

    private bool sequenceStarted = false;

    void Update()
    {
        if (!sequenceStarted && raycastSystem != null && raycastSystem.canActivateWin)
        {
            sequenceStarted = true; // aseguramos que solo se ejecute una vez
            StartCoroutine(PlaySequence());
        }
    }

    IEnumerator PlaySequence()
    {
        // Primera animación
        if (M_animator != null)
            M_animator.SetTrigger("TrMission");
        yield return new WaitForSeconds(waitBetweenAnims);

        // Segunda animación
        if (W_animator != null)
            W_animator.SetTrigger("TrMission");
        yield return new WaitForSeconds(waitBeforeScene);

        if (Fade_animator != null)
            Fade_animator.SetTrigger("TrFade");

        yield return new WaitForSeconds(waitBeforeScene);

        // Cambio de escena
        SceneManager.LoadScene(sceneName);
    }
}
