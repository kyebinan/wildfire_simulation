using UnityEngine;

/// <summary>
/// Altitude-only PD controller for a hexacopter.
/// Based on the model: T = (g + Kd(z_dot_d - z_dot) + Kp(z_d - z)) * m / (cos(phi) * cos(theta))
/// This class computes the required total thrust T.
/// </summary>
public class AltitudeController
{
    private Rigidbody rb;                  // Reference to the Rigidbody (for mass)
    private Transform droneTransform;      // Reference to get the current altitude (Y position)

    private float Kp;                      // Proportional gain
    private float Kd;                      // Derivative gain

    private float targetAltitude;          // Desired altitude in meters
    private float currentThrust;           // Output thrust

    public AltitudeController(Rigidbody rb, Transform droneTransform, float targetAltitude, float Kp, float Kd)
    {
        this.rb = rb;
        this.droneTransform = droneTransform;
        this.targetAltitude = targetAltitude;
        this.Kp = Kp;
        this.Kd = Kd;
    }

    public void UpdateController()
    {
        float g = 9.81f;                  // Gravity
        float mass = rb.mass;
        float z = droneTransform.position.y;                  // current altitude
        float zd = targetAltitude;                           // desired altitude
        float zDot = rb.velocity.y;                          // vertical speed
        float zDot_d = 0f;                                   // desired vertical speed (hover)

        float error = zd - z;
        float errorDot = zDot_d - zDot;

        float phi   = droneTransform.eulerAngles.z; if (phi > 180f) phi -= 360f;
        float theta = droneTransform.eulerAngles.x; if (theta > 180f) theta -= 360f;

        float cPhi  = Mathf.Cos(phi   * Mathf.Deg2Rad);
        float cTheta= Mathf.Cos(theta * Mathf.Deg2Rad);

        // thrust compensation
        float T = (g + Kd * errorDot + Kp * error) * mass / (cPhi * cTheta);

        currentThrust = Mathf.Max(T, 0f); // clamp negative thrust
    }

    public float GetRequiredTotalThrust()
    {
        return currentThrust;
    }

    public void SetTargetAltitude(float newTarget)
    {
        targetAltitude = newTarget;
    }
} 
