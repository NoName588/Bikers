using UnityEngine;

public class BikeMove : MonoBehaviour
{
    [Header("Velocidad y Motor")]
    public float acceleration = 10f;
    public float maxSpeed = 50f;
    public float brakeForce = 20f;
    public float naturalDeceleration = 2f;
    public float maxRPM = 10000f;

    [Header("Giro")]
    public float maxTurnSpeed = 80f;
    public float minTurnSpeed = 20f;
    public float maxLeanAngle = 30f;

    [Header("Transmisión")]
    public int currentGear = 1;
    public int maxGear = 7;
    public float[] gearRatios = { 0f, 0f, 0.15f, 0.25f, 0.4f, 0.6f, 0.8f, 1f };
    public float[] gearTorques = { 0f, 0.9f, 0.8f, 0.6f, 0.45f, 0.35f, 0.25f };

    [Header("Manubrio")]
    public Transform handlebar;       
    public float maxHandlebarTurn = 30f; 

    private float currentSpeed = 0f;
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

        if (Input.GetKey(KeyCode.W))
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

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, gearMaxSpeed);

        if (!(currentGear > 1 && currentSpeed < 5f))
            currentRPM = Mathf.Lerp(currentRPM, (currentSpeed / gearMaxSpeed) * maxRPM, Time.deltaTime * 5f);
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

    private void UpdateHandlebar() //Ayuda de Chat mamacita hermosa divina
    {
        if (handlebar != null)
        {
            float turnInput = Input.GetAxis("Horizontal");
            float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);

            // A baja velocidad, gira más el manubrio, a alta velocidad menos
            float handlebarAngle = turnInput * Mathf.Lerp(maxHandlebarTurn, maxHandlebarTurn / 5f, speedFactor);

            // Aplicar rotación en Z sin alterar X ni Y originales
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

    void OnGUI() // Recomendacion de la mamacita, luego la borro cuando tenga el Velocimentro, RPM y UI de cambios
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Gear: " + currentGear);
        GUI.Label(new Rect(10, 30, 200, 20), "Speed: " + currentSpeed.ToString("F1") + " km/h");
        GUI.Label(new Rect(10, 50, 200, 20), "RPM: " + currentRPM.ToString("F0"));

    }
}



