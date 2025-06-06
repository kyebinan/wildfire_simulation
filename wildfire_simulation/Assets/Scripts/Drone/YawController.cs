using UnityEngine;

/// <summary>
/// Yaw-only PD controller for a hexacopter.
/// Computes the yaw torque (tau_psi):
/// tau_psi = (Kd * (psidot_d - psidot) + Kp * (psi_d - psi)) * Izz
/// </summary>
public class YawController
{
    private float Kp;
    private float Kd;
    private float targetYaw;
    private float currentTorque;

    private Rigidbody rb;
    private Transform droneTransform;

    public YawController(Rigidbody rb, Transform droneTransform, float targetYaw, float Kp, float Kd)
    {
        this.rb = rb;
        this.droneTransform = droneTransform;
        this.targetYaw = targetYaw;
        this.Kp = Kp;
        this.Kd = Kd;
    }

    public void UpdateController()
    {
        float psi = droneTransform.eulerAngles.y;
        if (psi > 180f) psi -= 360f;

        float psidot = rb.angularVelocity.y;
        float psi_d = targetYaw;
        float psidot_d = 0f;

        float error = psi_d - psi;
        float errorDot = psidot_d - psidot;

        float Izz = rb.inertiaTensor.z;

        currentTorque = (Kd * errorDot + Kp * error) * Izz;
    }

    public float GetRequiredYawTorque()
    {
        return currentTorque;
    }

    public void SetTargetYaw(float newTarget)
    {
        targetYaw = newTarget;
    }
}

