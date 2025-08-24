using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class TagObjectPair
{
    public string tag;          // El Tag que se busca
    public GameObject target;   // El GameObject que se activará
    [HideInInspector] public bool activated = false; // Control interno para evitar reactivaciones
}

public class RaycastSystem : MonoBehaviour
{
    [Header("Raycast Settings")]
    public Camera playerCamera;
    public float rayDistance = 10f;
    private Texture2D centerPointTexture;
    private string currentTag = "";

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
    private bool canActivateWin = false;

    void Start()
    {
        CreateCenterPoint();
        UpdateUniqueTagsCount(); // Inicializa UI
    }

    void Update()
    {
        if (!isSystemFrozen)
        {
            UpdateRaycastTag();
            CheckForClick();
        }

        if (canActivateWin && Input.GetKeyDown(KeyCode.Space))
        {
            LoadIntroScene();
        }
    }

    void UpdateRaycastTag()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            currentTag = hit.collider.tag;
            tagText.text = "Parte: " + currentTag;
        }
        else
        {
            currentTag = "";
            tagText.text = "Parte: None";
        }

        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
    }

    void CheckForClick()
    {
        if (Input.GetMouseButtonDown(0) && !string.IsNullOrEmpty(currentTag))
        {
            if (!uniqueTags.Contains(currentTag))
            {
                uniqueTags.Add(currentTag);
                UpdateUniqueTagsCount();
            }

            // Buscar en la lista si hay un objeto con ese Tag
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
            pair.activated = true;
        }
    }

    void UpdateUniqueTagsCount()
    {
        uniqueTagsText.text = "Chequeo: " + uniqueTags.Count + "/" + tagObjectPairs.Count;

        if (uniqueTags.Count >= tagObjectPairs.Count)
        {
            FinalUI.SetActive(true);
            canActivateWin = true;
            Debug.Log("¡Has activado todos los objetos requeridos! Presiona espacio para cambiar de escena.");
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
        float pointSize = 8f;
        float x = (Screen.width / 2) - (pointSize / 2);
        float y = (Screen.height / 2) - (pointSize / 2);

        GUI.DrawTexture(new Rect(x, y, pointSize, pointSize), centerPointTexture);
    }
}
