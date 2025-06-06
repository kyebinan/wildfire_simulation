using UnityEngine;

/// <summary>
/// Pitch-only PD controller for a hexacopter.
/// Computes the pitch torque (tau_theta):
/// tau_theta = (Kd * (thetadot_d - thetadot) + Kp * (theta_d - theta)) * Iyy
/// </summary>
public class PitchController
{
    private float Kp;
    private float Kd;
    private float targetPitch;
    private float currentTorque;

    private Rigidbody rb;
    private Transform droneTransform;

    public PitchController(Rigidbody rb, Transform droneTransform, float targetPitch, float Kp, float Kd)
    {
        this.rb = rb;
        this.droneTransform = droneTransform;
        this.targetPitch = targetPitch;
        this.Kp = Kp;
        this.Kd = Kd;
    }

    public void UpdateController()
    {
        float theta = droneTransform.eulerAngles.x;
        if (theta > 180f) theta -= 360f; // wrap to [-180, 180]

        float thetadot = rb.angularVelocity.x;
        float theta_d = targetPitch;
        float thetadot_d = 0f;

        float error = theta_d - theta;
        float errorDot = thetadot_d - thetadot;

        float Iyy = rb.inertiaTensor.y;

        currentTorque = (Kd * errorDot + Kp * error) * Iyy;
    }

    public float GetRequiredPitchTorque()
    {
        return currentTorque;
    }

    public void SetTargetPitch(float newTarget)
    {
        targetPitch = newTarget;
    }
}
