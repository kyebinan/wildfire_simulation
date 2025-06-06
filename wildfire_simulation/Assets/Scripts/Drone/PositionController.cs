using UnityEngine;

/// <summary>
/// Position controller (PD) – convert a world‑frame (x,z) target into roll / pitch targets.
/// Outer‑loop PD :
///   pitchTarget = Kp * fwdError  − Kd * fwdSpeed_body
///   rollTarget  = Kp * rightError − Kd * rightSpeed_body
/// </summary>
public class PositionController
{
    // ───────── public tuning parameters ─────────
    public float Kp = 2.0f;    // proportional on position error (deg per metre)
    public float Kd = 1.0f;    // derivative on body‑frame velocity (deg per m/s)
    public float maxAngle = 20f; // mechanical/flight envelope limit (deg)

    // ───────── state ─────────
    private Vector2 targetXZ;        // desired world (x,z)
    private float pitchTargetDeg;    // + = nose‑up  (Unity X‑axis)
    private float rollTargetDeg;     // + = right‑wing‑down (Unity Z‑axis)

    private readonly Rigidbody rb;   // to read world velocity

    public PositionController(Rigidbody rb, Vector2 targetXZ, float kp, float kd)
    {
        this.targetXZ = targetXZ;
        this.Kp       = kp;
        this.Kd       = kd;
        this.rb       = rb;
    }

    // change the waypoint on the fly
    public void SetTarget(Vector2 newTarget) => targetXZ = newTarget;

    /// <summary>
    /// Compute pitch / roll references (deg).
    /// Call every FixedUpdate.
    /// </summary>
    /// <param name="droneTransform">Transform of the drone</param>
    public void UpdateController(Transform droneTransform)
    {
        // ----- position error in world frame -----
        Vector3 pos = droneTransform.position;
        float dx = targetXZ.x - pos.x;   // +east (Unity +X)
        float dz = targetXZ.y - pos.z;   // +north (Unity +Z)

        // ----- yaw to rotate world error into body frame -----
        float yawRad = Mathf.Deg2Rad * GetYawDeg(droneTransform);
        float fwdErr   =  Mathf.Cos(yawRad) * dz - Mathf.Sin(yawRad) * dx;  // body +X (nose)
        float rightErr =  Mathf.Cos(yawRad) * dx + Mathf.Sin(yawRad) * dz;  // body +Z (right wing)

        // ----- body‑frame velocities -----
        Vector3 vWorld = rb.velocity;
        float vFwd   =  Mathf.Cos(yawRad) * vWorld.z - Mathf.Sin(yawRad) * vWorld.x;
        float vRight =  Mathf.Cos(yawRad) * vWorld.x + Mathf.Sin(yawRad) * vWorld.z;

        // ----- PD law → angle targets (deg) -----
        pitchTargetDeg = Mathf.Clamp(Kp * fwdErr  - Kd * vFwd  , -maxAngle, maxAngle);
        rollTargetDeg  = Mathf.Clamp(Kp * rightErr - Kd * vRight, -maxAngle, maxAngle);
    }

    public float GetPitchTarget() => pitchTargetDeg;        // Unity X‑axis   (+ = nose‑up)
    public float GetRollTarget()  => -rollTargetDeg;         // Unity Z‑axis   (+ = right‑wing‑down)

    // ---------- helpers ----------
    private static float GetYawDeg(Transform tr)
    {
        Vector3 fwd = tr.forward;  fwd.y = 0f; fwd.Normalize();
        return Vector3.SignedAngle(Vector3.forward, fwd, Vector3.up); // [-180,180]
    }

} // class
