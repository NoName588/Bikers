using UnityEngine;
using TMPro; // Necesario para TextMeshPro

public class BikeUI : MonoBehaviour
{
    [Header("Referencia de la moto")]
    public BikeMove bike; // Arrastra aquí el objeto con BikeMove

    [Header("UI Elements")]
    public TMP_Text speedText;
    public TMP_Text tempText;
    public TMP_Text gearText;

    void Update()
    {
        if (bike == null) return;

        // Velocidad en km/h → entero
        if (speedText != null)
            speedText.text = bike.SpeedKmh.ToString("F0");

        // Temperatura motor → entero
        if (tempText != null)
            tempText.text = bike.EngineTemp.ToString("F0");

        // Marcha actual → si es 0, mostrar "N"
        if (gearText != null)
        {
            if (bike.CurrentGear == 0)
                gearText.text = "N";
            else
                gearText.text = bike.CurrentGear.ToString();
        }
    }
}


