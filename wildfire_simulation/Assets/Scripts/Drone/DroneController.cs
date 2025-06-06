using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

/// <summary>
/// HexaCopterController controls the flight behavior of a six-motor drone,
/// using PID controllers for position, altitude, and orientation.
/// Requires Rigidbody for physical simulation.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    Rigidbody rb;
    readonly Transform[] motorXf = new Transform[6]; // Motor transforms (0–5)
    readonly bool[] motorCW = { false, true, false, true, false, true }; // Clockwise rotation flags
    float[] motorOmega = new float[6]; // Motor angular speeds

    // Physical constants
    const float b = 1.7e-4f;   // Drag torque coefficient
    const float l = 0.3f;      // Distance from center to motor
    const float kLift = 6e-5f; // Lift constant

    // Control system components
    AltitudeController altitudeCtrl;
    RollController rollController;
    PitchController pitchController;
    YawController yawController;
    PositionController positionController;

    // Target states
    float pitchTarget, rollTarget, yawTarget;
    float targetAltitude;
    Vector2 positionTarget;

    // Control flags and positions
    private bool startMotors = false;
    private Vector3 landingStation;  // Initial position (landing pad)
    private Vector3 destination;     // Target destination (unused yet)
    private Battery batteryComponent;
    private RfModule rfComponent;
    private int indexArea = 0;
    private float altitude = 0f;
    private float drainFactor;
    public Queue<Vector3> subGoals = new Queue<Vector3>();
    public bool hasSubGoals => subGoals.Count > 0;

    private List<Vector3> areas;

    // ───────── Unity Lifecycle ─────────
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Choose a drainFactor randomly to simulate the battery's age
        drainFactor = Mathf.Lerp(1e-5f, 1e-4f, UnityEngine.Random.value);

        // Set initial targets
        landingStation = transform.position;
        pitchTarget = rollTarget = yawTarget = 0f;
        targetAltitude = transform.position.y;
        positionTarget = new Vector2(transform.position.x, transform.position.z);

        // Initialize controllers with smooth, damped gains
        altitudeCtrl = new AltitudeController(rb, transform, targetAltitude, 1.0f, 3.0f); // Kp=1, Kd=3
        rollController = new RollController(rb, transform, rollTarget, 1.5f, 4.0f);         // Kp=1.5, Kd=4
        pitchController = new PitchController(rb, transform, pitchTarget, 1.5f, 4.0f);       // Kp=1.5, Kd=4
        yawController = new YawController(rb, transform, yawTarget, 1.0f, 3.0f);           // Kp=1, Kd=3
        positionController = new PositionController(rb, positionTarget, 1.0f, 4.0f);           // Kp=1, Kd=4


        // Find the Battery script 
        batteryComponent = GetComponent<Battery>();
        if (batteryComponent == null)
            Debug.LogWarning("[DroneController] Battery component not found.");

        // Find the RfModule script 
        rfComponent = GetComponent<RfModule>();
        if (rfComponent == null)
            Debug.LogWarning("[DroneController] RfModule component not found.");

        // Find motor transforms
        for (int i = 0; i < 6; i++)
        {
            motorXf[i] = transform.Find($"Industrial drone_low.00{i + 1}");
            if (motorXf[i] == null)
                Debug.LogError($"[DroneController] {name}: Motor index {i} not found – check hierarchy!");
        }
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (batteryComponent.GetBatteryLevel() != 0f)
        {
            // ─── Step 1: Position Control (Outer loop) ───
            positionController.SetTarget(new Vector2(destination.x, destination.z));
            positionController.UpdateController(transform);
            pitchTarget = positionController.GetPitchTarget();
            rollTarget = positionController.GetRollTarget();

            // ─── Step 2: Attitude Control (PID Inner loop) ───
            pitchController.SetTargetPitch(pitchTarget);
            rollController.SetTargetRoll(rollTarget);
            pitchController.UpdateController();
            rollController.UpdateController();
            yawController.UpdateController(); // Could be dynamic in future

            // ─── Step 3: Altitude Control ───
            altitudeCtrl.SetTargetAltitude(targetAltitude);
            altitudeCtrl.UpdateController();

            // ─── Step 4: Extract PID Outputs ───
            float tau_phi = rollController.GetRequiredRollTorque();     // Roll torque
            float tau_theta = pitchController.GetRequiredPitchTorque(); // Pitch torque
            float tau_psi = yawController.GetRequiredYawTorque();       // Yaw torque
            float thrust = altitudeCtrl.GetRequiredTotalThrust();       // Total thrust

            // ─── Step 5: Motor Speed Calculation ───
            UpdateMotorOmega(thrust, tau_phi, tau_theta, tau_psi);
            ApplyMotor(dt);

            // ─── Debugging ───
            if (startMotors)
            {
                // Debug.Log($"[DroneController] drone: {name} / Thrust: {thrust:F2}  τ_roll: {tau_phi:F2}  τ_pitch: {tau_theta:F2}  τ_yaw: {tau_psi:F2}");
                // Debug.Log($"[DroneController] drone: {name} / PitchTarget: {pitchTarget:F1}°  RollTarget: {rollTarget:F1}°");
            }
        }
    }

    /// <summary> Calculates motor speeds based on thrust and torques. </summary>
    void UpdateMotorOmega(float thrust, float tau_phi, float tau_theta, float tau_psi)
    {
        float k = kLift;

        motorOmega[0] = Mathf.Sqrt(Mathf.Max(0f, thrust / (6f * k) - (2f * tau_theta) / (5f * k * l) - tau_psi / (10f * b)));
        motorOmega[1] = Mathf.Sqrt(Mathf.Max(0f, thrust / (6f * k) + tau_phi / (3f * k * l) - tau_theta / (5f * k * l) + tau_psi / (5f * b)));
        motorOmega[2] = Mathf.Sqrt(Mathf.Max(0f, thrust / (6f * k) + tau_phi / (3f * k * l) + tau_theta / (5f * k * l) - tau_psi / (5f * b)));
        motorOmega[3] = Mathf.Sqrt(Mathf.Max(0f, thrust / (6f * k) + (2f * tau_theta) / (5f * k * l) + tau_psi / (10f * b)));
        motorOmega[4] = Mathf.Sqrt(Mathf.Max(0f, thrust / (6f * k) - tau_phi / (3f * k * l) + tau_theta / (5f * k * l) - tau_psi / (5f * b)));
        motorOmega[5] = Mathf.Sqrt(Mathf.Max(0f, thrust / (6f * k) - tau_phi / (3f * k * l) - tau_theta / (5f * k * l) + tau_psi / (5f * b)));
    }

    /// <summary> Applies total thrust to drone center of mass. </summary>
    void ApplyThrust()
    {
        Vector3 cgWorld = transform.TransformPoint(rb.centerOfMass);

        float thrust = 0f;
        for (int i = 0; i < 6; i++)
            thrust += motorOmega[i] * motorOmega[i];

        thrust *= kLift;
        rb.AddForceAtPosition(transform.up * thrust, cgWorld, ForceMode.Force);
        ConsumeBattery(thrust);
    }

    /// <summary> Applies torque based on differential motor speeds. </summary>
    void ApplyTorque()
    {
        float w1_2 = motorOmega[0] * motorOmega[0];
        float w2_2 = motorOmega[1] * motorOmega[1];
        float w3_2 = motorOmega[2] * motorOmega[2];
        float w4_2 = motorOmega[3] * motorOmega[3];
        float w5_2 = motorOmega[4] * motorOmega[4];
        float w6_2 = motorOmega[5] * motorOmega[5];
        float sin60f = Mathf.Sin(60f * Mathf.Deg2Rad);

        float tau_phi = sin60f * kLift * l * (w2_2 + w3_2 - w5_2 - w6_2); // Roll
        float tau_theta = kLift * l * (-w1_2 - 0.25f * w2_2 + 0.25f * w3_2 + w4_2 + 0.25f * w5_2 - 0.25f * w6_2); // Pitch
        float tau_psi = b * (-w1_2 + w2_2 - w3_2 + w4_2 - w5_2 + w6_2); // Yaw

        Vector3 torqueUnity = new Vector3(tau_theta, tau_psi, tau_phi);  // Unity uses (x, y, z)
        rb.AddTorque(torqueUnity, ForceMode.Force);
    }

    /// <summary> Applies thrust, torque and updates visual rotation for motors. </summary>
    void ApplyMotor(float dt)
    {
        if (!startMotors) return;

        ApplyThrust();
        ApplyTorque();

        for (int idx = 0; idx < 6; idx++)
        {
            Transform hub = motorXf[idx];
            if (hub == null) continue;

            float spinDeg = motorOmega[idx] * Mathf.Rad2Deg * dt * (motorCW[idx] ? 1f : -1f);
            hub.Rotate(Vector3.up, spinDeg, Space.Self);
        }
    }

    void ConsumeBattery(float thrust)
    {
        if (batteryComponent != null)
        {
            // Generate a random value between 1e-5f and 1e-4f using Mathf

            float estimatedDrain = thrust * drainFactor;
            batteryComponent.DrainBattery(estimatedDrain);
        }
    }



    // ───────── API Methods ─────────

public RfModule RFComponent
    {
        get { return rfComponent; }
        set { rfComponent = value; }
    }
    public List<Vector3> Areas
    {
        get { return areas; }
        set { areas = value; }
    }
    public int IndexArea
    {
        get { return indexArea; }
        set { indexArea = value; }
    }

    /// <summary>
    /// Gets or sets the target altitude of the drone.
    /// </summary>
    public float Altitude
    {
        get { return altitude; }
        set { altitude = value; }
    }

    /// <summary>
    /// Sets the target destination for the drone.
    /// </summary>
    public void SetDestination(Vector3 newDestination)
    {
        destination = newDestination;
        targetAltitude = destination.y;
    }

    /// <summary>
    /// Gets the current destination of the drone.
    /// </summary>
    public Vector3 GetDestination()
    {
        return destination;
    }

    /// <summary>
    /// Commands the drone to return to its original landing station.
    /// </summary>
    // public void ReturnToBase()
    // {
    //     destination = landingStation;
    // }

    /// <summary>
    /// Enables or disables the drone's motors.
    /// </summary>
    public void SetMotorState(bool enabled)
    {
        startMotors = enabled;
    }

    /// <summary>
    /// Returns whether the drone's motors are currently enabled.
    /// </summary>
    public bool GetMotorState()
    {
        return startMotors;
    }

    public Vector3 GetLandingStation()
    {
        return landingStation;
    }

    public float GetBatteryLevel()
    {
        return batteryComponent.GetBatteryLevel();
    }

    public void RechargeBatteryFull()
    {
        batteryComponent.RechargeFull();
    }

    public void RechargeBatteryIncrement(float amout)
    {
        batteryComponent.SetBatteryLevel(amout);
    }

    public void SetSubGoals(Vector3 start, Vector3 end, float stepSize = 10f)
    {
        subGoals.Clear();
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        for (float d = stepSize; d < distance; d += stepSize)
        {
            subGoals.Enqueue(start + direction * d);
        }

        subGoals.Enqueue(end); // Always enqueue the final destination
    }
    

}
