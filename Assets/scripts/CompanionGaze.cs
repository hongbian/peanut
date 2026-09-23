using UnityEngine;

// Cosmetic only: the heart's gaze telegraphs the lure mechanic before it starts
// drifting, so the player gets an early warning. Facing does NOT affect lure
// strength (yet) -- see CLAUDE.md open design questions.
public class CompanionGaze : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float turnSpeed = 3.5f;
    [SerializeField] private float moveThreshold = 0.05f;

    private CompanionFollow follow;
    private CompanionCorruption corruption;

void Start()
    {
        follow = GetComponent<CompanionFollow>();
        corruption = GetComponent<CompanionCorruption>();
    }

void Update()
    {
        Vector3 lookTarget = ChooseLookTarget();

        Vector3 flatDir = lookTarget - transform.position;
        flatDir.y = 0f; // Y-axis rotation only -- no tilting up/down

        // corruption makes the gaze dreamier and less responsive; at full corruption it stops turning entirely
        float effectiveTurnSpeed = corruption != null ? turnSpeed * Mathf.Lerp(1f, 0f, corruption.Corruption) : turnSpeed;

        if (flatDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, effectiveTurnSpeed * Time.deltaTime);
        }
    }

    private Vector3 ChooseLookTarget()
    {
        // carried: keep it simple, just look at the player
        if (!follow.enabled) return player.position;

        // a lure is pulling (or has captured it) -- gaze leads the drift, even mid-follow
        if (follow.CurrentLure != null)
            return follow.CurrentLure.transform.position;

        // moving under its own power: look where it's going
        if (follow.CurrentVelocity.sqrMagnitude > moveThreshold * moveThreshold)
            return transform.position + follow.CurrentVelocity.normalized * 2f;

        // idle: look at the player
        return player.position;
    }
}
