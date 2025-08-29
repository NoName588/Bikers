using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CarAIController : MonoBehaviour
{
    [Header("Wheel Setup")]
    [Tooltip("Si es true, el vehículo tendrá 6 ruedas. Si es false, tendrá 4.")]
    public bool isSixWheels = false;

    //Wheel transforms
    [Header("Wheel Transforms")]
    public Transform frontRight;
    public Transform frontLeft;
    public Transform rearRight;
    public Transform rearLeft;
    public Transform middleRight;   // Extra para 6 ruedas
    public Transform middleLeft;    // Extra para 6 ruedas

    //Wheel colliders
    [Header("Wheel Colliders")]
    public WheelCollider frontRightCollider;
    public WheelCollider frontLeftCollider;
    public WheelCollider rearRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider middleRightCollider;  // Extra para 6 ruedas
    public WheelCollider middleLeftCollider;   // Extra para 6 ruedas

    [Header("Checkpoints And Detections")]
    public Transform nextCheckpoint;
    public List<Transform> checks = new List<Transform> { null };
    public bool CheckPointSearch = true;
    public bool objectDetected = false;
    public bool isCarControlledByAI = true;
    public LayerMask seenLayers = Physics.AllLayers;

    [Header("Car Settings")]
    public int kmh;
    public int speedLimit;
    public float distanceFromObjects = 2f;
    public int recklessnessThreshold = 0;
    public bool despawnForFlippingOver = true;
    public bool taxiMode = false;
    public float acceleration = 100f;
    public float breaking = 1000f;

    private Stopwatch stopwatch = new Stopwatch();
    private Vector3 lastPos;
    private float steerAngle = 0f;
    private bool flipOverCheck = false;

    private void FixedUpdate()
    {
        // Actualizar ruedas
        WheelUpdate(frontRight, frontRightCollider);
        WheelUpdate(frontLeft, frontLeftCollider);
        WheelUpdate(rearRight, rearRightCollider);
        WheelUpdate(rearLeft, rearLeftCollider);

        if (isSixWheels)
        {
            WheelUpdate(middleRight, middleRightCollider);
            WheelUpdate(middleLeft, middleLeftCollider);
        }

        // Calcular velocidad
        CalculateKMH();

        // Buscar checkpoints
        SearchForCheckpoints();

        if (despawnForFlippingOver && !flipOverCheck)
        {
            flipOverCheck = true;
            StartCoroutine(CheckForFlippingOver());
        }
    }

    private void WheelUpdate(Transform transform, WheelCollider collider)
    {
        if (transform == null || collider == null) return;
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        transform.position = pos;
        transform.rotation = rot;
    }

    public void Accelerate(float value)
    {
        // Torque en las ruedas delanteras (motor delantero típico)
        frontRightCollider.motorTorque = value;
        frontLeftCollider.motorTorque = value;

        if (isSixWheels)
        {
            middleRightCollider.motorTorque = value;
            middleLeftCollider.motorTorque = value;
        }
    }

    public void Break(float value)
    {
        // Frenado en todas las ruedas
        frontRightCollider.brakeTorque = value;
        frontLeftCollider.brakeTorque = value;
        rearRightCollider.brakeTorque = value;
        rearLeftCollider.brakeTorque = value;

        if (isSixWheels)
        {
            middleRightCollider.brakeTorque = value;
            middleLeftCollider.brakeTorque = value;
        }
    }

    public void Turn(float angle)
    {
        // Solo giran las delanteras
        frontRightCollider.steerAngle = angle;
        frontLeftCollider.steerAngle = angle;
    }

    private void CalculateKMH()
    {
        if (stopwatch.IsRunning)
        {
            stopwatch.Stop();
            float distance = (transform.position - lastPos).magnitude;
            float time = (float)stopwatch.Elapsed.TotalSeconds; // CORREGIDO
            if (time > 0)
                kmh = (int)(3.6f * distance / time); // m/s → km/h
            lastPos = transform.position;
            stopwatch.Reset();
            stopwatch.Start();
        }
        else
        {
            lastPos = transform.position;
            stopwatch.Reset();
            stopwatch.Start();
        }
    }

    public void SetSpeed(int speedLimit)
    {
        if (kmh > speedLimit)
        {
            Break(breaking);
            Accelerate(0);
        }
        else if (kmh < speedLimit)
        {
            Accelerate(acceleration);
            Break(0);
        }
    }

    private void SearchForCheckpoints()
    {
        if (CheckPointSearch && isCarControlledByAI && nextCheckpoint != null)
        {
            Vector3 nextCheckpointRelative = transform.InverseTransformPoint(nextCheckpoint.position);

            steerAngle = nextCheckpointRelative.x / nextCheckpointRelative.magnitude;
            float xangle = nextCheckpointRelative.y / nextCheckpointRelative.magnitude;

            steerAngle = Mathf.Asin(steerAngle) * Mathf.Rad2Deg;
            xangle = Mathf.Asin(xangle) * Mathf.Rad2Deg;

            Turn(steerAngle);

            float maxDistance = kmh * kmh / 100f + distanceFromObjects;
            RaycastHit carHit;
            int objectInFront = 0;

            for (int i = 0; i < checks.Count; i++)
            {
                if (checks[i] == null) continue;
                checks[i].localRotation = Quaternion.Euler(-xangle, steerAngle, 0);
                bool isObjectInFront = Physics.Raycast(
                    checks[i].position,
                    checks[i].forward,
                    out carHit,
                    maxDistance,
                    seenLayers,
                    QueryTriggerInteraction.Ignore
                );

#if UNITY_EDITOR
                UnityEngine.Debug.DrawRay(checks[i].position, checks[i].forward * maxDistance, Color.green);
#endif

                if (isObjectInFront)
                    objectInFront++;
            }

            if (objectInFront > 0)
            {
                SetSpeed(0);
                objectDetected = true;
            }
            else
            {
                objectDetected = false;
                int speed = speedLimit + recklessnessThreshold;
                if (speedLimit == 0) speed = 0;
                if (speed == 0) speed = speedLimit;
                SetSpeed(speed);
            }
        }
    }

    IEnumerator CheckForFlippingOver()
    {
        bool deleteCar = isCarFlippedOver();

        if (deleteCar)
        {
            for (int i = 0; i < 10; i++)
            {
                if (!isCarFlippedOver())
                {
                    deleteCar = false;
                }
                yield return new WaitForSeconds(1);
            }

            if (deleteCar)
            {
                UnityEngine.Debug.Log("Car " + gameObject.name + " destroyed for flipping over.");
                Destroy(gameObject);
            }
        }

        yield return new WaitForSeconds(10);
        flipOverCheck = false;
    }

    private bool isCarFlippedOver()
    {
        return Vector3.Dot(transform.up, Vector3.down) > 0.7f;
    }
}
