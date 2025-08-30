using UnityEngine;
using UnityEngine.UI;

public class BikeUI_RPMBar : MonoBehaviour
{
    [Header("Referencia de la moto")]
    public BikeMove bike; // Arrastra aqu� el objeto con BikeMove

    [Header("Barras de RPM")]
    public Image[] rpmBars; // Asigna las 15 barras en orden en el Inspector

    [Header("Configuraci�n RPM")]
    public float maxRpmUI = 8000f; // L�mite m�ximo que representa las 15 barras

    void Update()
    {
        if (bike == null || rpmBars.Length == 0) return;

        // Calcular cu�ntas barras deben estar activas
        float normalizedRPM = Mathf.Clamp01(bike.CurrentRPM / maxRpmUI); // valor entre 0 y 1
        int activeBars = Mathf.RoundToInt(normalizedRPM * rpmBars.Length);

        // Activar/desactivar en secuencia
        for (int i = 0; i < rpmBars.Length; i++)
        {
            if (i < activeBars)
                rpmBars[i].enabled = true; // Mostrar barra
            else
                rpmBars[i].enabled = false; // Ocultar barra
        }
    }
}
