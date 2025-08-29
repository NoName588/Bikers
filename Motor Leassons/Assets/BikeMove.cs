using UnityEngine;

public class BikeMove : MonoBehaviour
{
    [Header("Velocidad y Motor")]
    public float acceleration = 10f;
    public float maxSpeed = 50f;
    public float brakeForce = 20f;
    public float naturalDeceleration = 2f;
    public float maxRPM = 10000f;

    [Header("Temperatura del motor")]
    public float engineTemperature = 70f;   // °C inicial (ralentí normal)
    public float minTemperature = 60f;      // Temperatura mínima (reposo)
    public float maxTemperature = 120f;     // Límite peligroso
    public float heatingRate = 0.05f;       // Cuánto sube por unidad de esfuerzo
    public float coolingRate = 0.02f;       // Cuánto baja por segundo en reposo

    [Header("Giro")]
    public float maxTurnSpeed = 80f;
    public float minTurnSpeed = 20f;
    public float maxLeanAngle = 30f;

    [Header("Transmisión")]
    public int currentGear = 1;
    public int maxGear = 7;

    // Arrays consistentes (mismo tamaño: 8 elementos → 0 = neutro)
    public float[] gearRatios = { 0f, 0.15f, 0.25f, 0.4f, 0.6f, 0.8f, 1f, 1.2f };
    public float[] gearTorques = { 0f, 0.9f, 0.8f, 0.6f, 0.45f, 0.35f, 0.25f, 0.2f };

    [Header("Manubrio")]
    public Transform handlebar;
    public float maxHandlebarTurn = 30f;

    private float currentSpeed = 0f;   // en m/s
    private float currentRPM;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        HandleGearShift();
        HandleAcceleration();
        HandleSteering();
        UpdateLean();
        UpdateHandlebar();
        UpdateEngineTemperature(); // ✅ ahora también se actualiza temperatura
    }

    void FixedUpdate()
    {
        MoveBike();
    }

    private void HandleGearShift()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentGear < maxGear)
            currentGear++;
        else if (Input.GetKeyDown(KeyCode.Q) && currentGear > 0)
            currentGear--;
    }

    private void HandleAcceleration()
    {
        float gearMaxSpeed = maxSpeed * gearRatios[currentGear];
        float gearTorque = gearTorques[currentGear];

        // --- ACELERACIÓN DE VELOCIDAD ---
        if (Input.GetKey(KeyCode.W) && currentGear > 0) // Solo si hay marcha engranada
        {
            if (currentSpeed < gearMaxSpeed && !(currentGear > 1 && currentSpeed < 5f))
                currentSpeed += acceleration * gearTorque * Time.deltaTime;
            else if (currentGear > 1 && currentSpeed < 5f)
                currentRPM = Mathf.Lerp(currentRPM, 3000f, Time.deltaTime * 2f);
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            currentSpeed -= brakeForce * Time.deltaTime;
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, naturalDeceleration * Time.deltaTime);
        }

        // Ajuste progresivo de velocidad al cambiar de marcha
        if (currentSpeed > gearMaxSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, gearMaxSpeed, Time.deltaTime * 2f);
        }

        // --- CÁLCULO DE RPM ---
        if (currentGear == 0) // ✅ En neutro → RPM solo responde al acelerador
        {
            if (Input.GetKey(KeyCode.W))
            {
                currentRPM = Mathf.Lerp(currentRPM, maxRPM, Time.deltaTime * 3f);
            }
            else
            {
                currentRPM = Mathf.Lerp(currentRPM, 1000f, Time.deltaTime * 2f);
            }
        }
        else // ✅ En marcha engranada → RPM depende de la velocidad
        {
            if (!(currentGear > 1 && currentSpeed < 5f))
            {
                float normalizedSpeed = Mathf.Clamp01(currentSpeed / gearMaxSpeed);
                float targetRPM = Mathf.Lerp(1000f, maxRPM, normalizedSpeed);

                currentRPM = Mathf.Lerp(currentRPM, targetRPM, Time.deltaTime * 5f);
            }
            else
            {
                currentRPM = Mathf.Lerp(currentRPM, 1000f, Time.deltaTime * 2f);
            }
        }
    }

    private void HandleSteering()
    {
        if (currentSpeed > 0.1f)
        {
            float turnInput = Input.GetAxis("Horizontal");
            float speedFactor = currentSpeed / maxSpeed;
            float dynamicTurnSpeed = Mathf.Lerp(maxTurnSpeed, minTurnSpeed, speedFactor);
            transform.Rotate(Vector3.up, turnInput * dynamicTurnSpeed * Time.deltaTime);
        }
    }

    private void UpdateLean()
    {
        float turnDirection = Input.GetAxis("Horizontal");
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);
        float targetLeanAngle = -turnDirection * maxLeanAngle * speedFactor;
        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, targetLeanAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    private void UpdateHandlebar()
    {
        if (handlebar != null)
        {
            float turnInput = Input.GetAxis("Horizontal");
            float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);

            float handlebarAngle = turnInput * Mathf.Lerp(maxHandlebarTurn, maxHandlebarTurn / 5f, speedFactor);

            Vector3 localEuler = handlebar.localEulerAngles;
            localEuler.z = handlebarAngle;
            handlebar.localEulerAngles = localEuler;
        }
    }

    private void MoveBike()
    {
        Vector3 forwardMovement = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMovement);
    }

    // ✅ SISTEMA DE TEMPERATURA DEL MOTOR
    private void UpdateEngineTemperature()
    {
        // Normalizamos esfuerzo (0 a 1) según RPM
        float effort = Mathf.InverseLerp(1000f, maxRPM, currentRPM);

        if (Input.GetKey(KeyCode.W) && currentGear > 0)
        {
            // Acelerar → sube la temperatura
            engineTemperature += heatingRate * effort * Time.deltaTime * 100f;
        }
        else
        {
            // Enfriamiento natural
            engineTemperature -= coolingRate * Time.deltaTime * 100f;
        }

        // Clamps
        engineTemperature = Mathf.Clamp(engineTemperature, minTemperature, maxTemperature + 20f);

        // Si se excede el límite peligroso
        if (engineTemperature >= maxTemperature)
        {
            Debug.LogWarning("⚠️ El motor está en sobrecalentamiento! (" + engineTemperature.ToString("F1") + " °C)");
        }
    }

    void OnGUI()
    {
        float speedKmh = currentSpeed * 3.6f; // ✅ conversión correcta m/s → km/h

        GUI.Label(new Rect(10, 10, 200, 20), "Gear: " + currentGear);
        GUI.Label(new Rect(10, 30, 200, 20), "Speed: " + speedKmh.ToString("F1") + " km/h");
        GUI.Label(new Rect(10, 50, 200, 20), "RPM: " + currentRPM.ToString("F0"));
        GUI.Label(new Rect(10, 70, 200, 20), "Temp: " + engineTemperature.ToString("F1") + " °C"); // ✅ Temperatura
    }
}


