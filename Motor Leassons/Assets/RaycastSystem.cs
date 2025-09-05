using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class TagObjectPair
{
    public string tag;          // Tag a detectar
    public GameObject target;   // Objeto que se activará al hacer clic
    public GameObject NoCheck;  // UI al mirar el tag (antes del clic)
    public GameObject Check;    // UI de completado (tras re-entrar luego del clic)

    [HideInInspector] public bool activated = false;         // Fue clickeado
    [HideInInspector] public bool waitingForReenter = false; // Espera salir y re-entrar
    [HideInInspector] public bool exitedAfterClick = false;  // Ya salió al menos 1 vez tras el clic
}

public class RaycastSystem : MonoBehaviour
{
    [Header("Raycast Settings")]
    public Camera playerCamera;
    public float rayDistance = 10f;
    private Texture2D centerPointTexture;
    private string currentTag = "";
    private string lastTag = "";
    private bool showPointer = true;

    [Header("UI")]
    public TextMeshProUGUI tagText;
    public TextMeshProUGUI uniqueTagsText;
    public GameObject FinalUI;

    [Header("Game Flow")]
    public GameObject WIN;
    public bool isSystemFrozen = false;

    [Header("Tags & Objects")]
    public List<TagObjectPair> tagObjectPairs = new List<TagObjectPair>();

    private HashSet<string> uniqueTags = new HashSet<string>();
    public bool canActivateWin = false;

    void Start()
    {
        CreateCenterPoint();
        UpdateUniqueTagsCount();

        foreach (var pair in tagObjectPairs)
        {
            if (pair.NoCheck != null) pair.NoCheck.SetActive(false);
            if (pair.Check != null) pair.Check.SetActive(false);
            pair.activated = false;
            pair.waitingForReenter = false;
            pair.exitedAfterClick = false;
        }

        lastTag = "";
        UpdateCursorState();
    }

    void Update()
    {
        if (!isSystemFrozen)
        {
            UpdateRaycastTag();
            CheckForClick();
        }

        UpdateCursorState();

        /*if (canActivateWin && Input.GetKeyDown(KeyCode.Space))
        {
            LoadIntroScene();
        }*/
    }

    void UpdateRaycastTag()
    {
        // ——— Raycast desde el centro de la cámara
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, rayDistance);

        // ——— Detectar tag actual
        if (hitSomething)
        {
            currentTag = hit.collider.tag;
            tagText.text = "Parte: " + currentTag;
        }
        else
        {
            currentTag = "";
            tagText.text = "Parte: None";
        }

        // ——— Apagar todos los NoCheck y Check (se recalculan cada frame)
        foreach (var pair in tagObjectPairs)
        {
            if (pair.NoCheck != null) pair.NoCheck.SetActive(false);
            if (pair.Check != null) pair.Check.SetActive(false);
        }

        // ——— Lógica al mirar un tag conocido
        bool tagEncontrado = false;
        if (!string.IsNullOrEmpty(currentTag))
        {
            var activePair = tagObjectPairs.Find(p => p.tag == currentTag);
            if (activePair != null)
            {
                tagEncontrado = true;

                if (!activePair.activated)
                {
                    // Antes del clic → mostrar NoCheck
                    if (activePair.NoCheck != null) activePair.NoCheck.SetActive(true);
                }
                else
                {
                    // Después de clic → Check se comporta como NoCheck
                    if (activePair.Check != null) activePair.Check.SetActive(true);
                }
            }
        }

        // ——— Pointer visible solo si no miramos un tag válido
        showPointer = !tagEncontrado;

        // ——— Debug visual del raycast
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);

        // ——— Guardar tag actual como "último tag" para el próximo frame
        lastTag = currentTag;
    }


    void CheckForClick()
    {
        if (Input.GetMouseButtonDown(0) && !string.IsNullOrEmpty(currentTag))
        {
            if (!uniqueTags.Contains(currentTag))
            {
                uniqueTags.Add(currentTag);
                // ya no dependemos únicamente de esto, pero lo dejamos si lo quieres mantener
                //UpdateUniqueTagsCount(); // <- opcional dejar comentado
            }

            foreach (var pair in tagObjectPairs)
            {
                if (pair.tag == currentTag && !pair.activated)
                {
                    ActivateObject(pair);
                    break;
                }
            }
        }
    }

    void ActivateObject(TagObjectPair pair)
    {
        if (pair.target != null && !pair.target.activeSelf)
        {
            pair.target.SetActive(true);
        }

        // Si el target tiene DialogueSystem, le asignamos este RaycastSystem
        if (pair.target != null)
        {
            DialogueSystem ds = pair.target.GetComponentInChildren<DialogueSystem>();
            if (ds != null)
            {
                ds.raycastScript = this; // importante para notificar luego
            }
        }

        // Marcar activado y preparar “esperar re-entrada”
        pair.activated = true;
        pair.waitingForReenter = true;
        pair.exitedAfterClick = false;

        // Apagar NoCheck inmediatamente tras el clic
        if (pair.NoCheck != null) pair.NoCheck.SetActive(false);

        // NO encendemos Check aquí; se encenderá cuando el ray vuelva a tocar el objeto
        if (pair.Check != null) pair.Check.SetActive(false);
    }

    /// <summary>
    /// Recalcula el número de targets cuyos DialogueSystem.ReadyToContinue == true
    /// </summary>
    public void RecalculateReadyCount()
    {
        UpdateUniqueTagsCount();
    }

    public void UpdateUniqueTagsCount()
    {
        int readyCount = 0;

        foreach (var pair in tagObjectPairs)
        {
            if (pair.target == null) continue;

            // Buscamos DialogueSystem en el target (o hijos)
            DialogueSystem dialogue = pair.target.GetComponentInChildren<DialogueSystem>();
            if (dialogue != null && dialogue.ReadyToContinue)
            {
                readyCount++;
            }
        }

        uniqueTagsText.text = "Chequeo: " + readyCount + "/" + tagObjectPairs.Count;

        if (readyCount >= tagObjectPairs.Count)
        {
            //if (FinalUI != null) FinalUI.SetActive(true);
            canActivateWin = true;
            Debug.Log("✅ ¡Todos los diálogos confirmados! Presiona espacio para cambiar de escena.");
        }
    }


    void LoadIntroScene()
    {
        SceneManager.LoadScene("Carretera");
    }

    void CreateCenterPoint()
    {
        centerPointTexture = new Texture2D(1, 1);
        centerPointTexture.SetPixel(0, 0, Color.white);
        centerPointTexture.Apply();
    }

    void OnGUI()
    {
        // Si el pointer está desactivado o el sistema está congelado → no dibujar
        if (!showPointer || isSystemFrozen) return;

        float pointSize = 8f;
        float x = (Screen.width / 2f) - (pointSize / 2f);
        float y = (Screen.height / 2f) - (pointSize / 2f);

        GUI.DrawTexture(new Rect(x, y, pointSize, pointSize), centerPointTexture);
    }


    void UpdateCursorState()
    {
        if (isSystemFrozen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
