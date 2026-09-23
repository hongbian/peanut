using UnityEngine;

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
        flatDir.y = 0f; 

        float effectiveTurnSpeed = corruption != null ? turnSpeed * Mathf.Lerp(1f, 0f, corruption.Corruption) : turnSpeed;

        if (flatDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, effectiveTurnSpeed * Time.deltaTime);
        }
    }

    private Vector3 ChooseLookTarget()
    {
        if (!follow.enabled) return player.position;

        if (follow.CurrentLure != null)
            return follow.CurrentLure.transform.position;

        if (follow.CurrentVelocity.sqrMagnitude > moveThreshold * moveThreshold)
            return transform.position + follow.CurrentVelocity.normalized * 2f;

        return player.position;
    }
}
