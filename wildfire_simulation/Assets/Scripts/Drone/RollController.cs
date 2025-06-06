using UnityEngine;

/// <summary>
/// Roll-only PD controller for a hexacopter.
/// Computes the roll torque (tau_phi) using:
/// tau_phi = (Kd * (phidot_d - phidot) + Kp * (phi_d - phi)) * Ixx
/// </summary>
public class RollController
{
    private float Kp;
    private float Kd;
    private float targetRoll;
    private float currentTorque;

    private Rigidbody rb;
    private Transform droneTransform;

    public RollController(Rigidbody rb, Transform droneTransform, float targetRoll, float Kp, float Kd)
    {
        this.rb = rb;
        this.droneTransform = droneTransform;
        this.targetRoll = targetRoll;
        this.Kp = Kp;
        this.Kd = Kd;
    }

    public void UpdateController()
    {
        float phi = droneTransform.eulerAngles.z;
        if (phi > 180f) phi -= 360f; // wrap to [-180, 180]

        float phidot = rb.angularVelocity.z;
        float phi_d = targetRoll;
        float phidot_d = 0f;

        float error = phi_d - phi;
        float errorDot = phidot_d - phidot;

        float Ixx = rb.inertiaTensor.x;

        currentTorque = (Kd * errorDot + Kp * error) * Ixx;
    }

    public float GetRequiredRollTorque()
    {
        return currentTorque;
    }

    public void SetTargetRoll(float newTarget)
    {
        targetRoll = newTarget;
    }
} 
