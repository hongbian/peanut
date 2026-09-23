using UnityEngine;

// A beautiful object that pulls the companion toward it. Threats in this game
// don't chase -- they lure. Pull scales from 0 at the edge of lureRadius to
// full strength at the center; inside captureRadius the heart stands still,
// mesmerized, until the player picks it up.
public class HeartLure : MonoBehaviour
{
    [SerializeField] private float lureRadius = 4f;
    [SerializeField] private float captureRadius = 0.8f;
    [SerializeField] private float lureStrength = 1.2f;

    public Vector3 GetPull(Vector3 fromPosition, out bool isCaptured)
    {
        Vector3 toLure = transform.position - fromPosition;
        toLure.y = 0f;
        float dist = toLure.magnitude;

        if (dist >= lureRadius)
        {
            isCaptured = false;
            return Vector3.zero;
        }

        isCaptured = dist <= captureRadius;
        if (isCaptured) return Vector3.zero;

        float t = 1f - (dist / lureRadius); // 0 at edge, 1 at center
        return toLure.normalized * lureStrength * t;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.3f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, lureRadius);
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, captureRadius);
    }
}
