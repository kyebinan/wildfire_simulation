using UnityEngine;

public class JammerArea : MonoBehaviour
{
    [Header("Jamming Settings")]
    [Tooltip("Effective radius of the jamming zone (in meters).")]
    public float radius = 50f;

    [Tooltip("Noise floor override when inside this zone (in dBm).")]
    public float noiseFloorDbm = -70f;

    /// <summary>
    /// Check if a position is inside the jammer area.
    /// </summary>
    public bool IsInside(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= radius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
