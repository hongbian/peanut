using UnityEngine;

public class CompanionFollow : MonoBehaviour
{
    [SerializeField] private Transform target;        
    [SerializeField] private float followDistance = 1.5f;
    [SerializeField] private float wakeDistance = 2.5f;  
    [SerializeField] private float speed = 2.6f;       
    [SerializeField] private float smoothTime = 0.25f; 
    [SerializeField] private float willpower = 0.75f;   

    private Vector3 currentVelocity;
    private bool isFollowing;
    private bool isSitting;
    private CompanionCorruption corruption;

    public bool IsMesmerized { get; private set; }
    public bool IsSitting => isSitting;
    public HeartLure CurrentLure { get; private set; } 
    public Vector3 CurrentVelocity => currentVelocity;
    public float FollowDistance => followDistance;
    public float CurrentPullStrength { get; private set; } 

    public void SetSitting(bool sitting)
    {
        isSitting = sitting;
        if (sitting)
        {
            isFollowing = false;
            currentVelocity = Vector3.zero;
        }
    }

void Update()
    {
        Vector3 desiredVelocity = Vector3.zero;

        if (!isSitting)
        {
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f; 
            float dist = toTarget.magnitude;

            if (dist > wakeDistance) isFollowing = true;
            if (dist < followDistance) isFollowing = false;

            float corruptionFactor = corruption != null ? Mathf.Lerp(1f, 0f, corruption.Corruption) : 1f;
            desiredVelocity = isFollowing ? toTarget.normalized * speed * corruptionFactor : Vector3.zero;
        }

        Vector3 lurePull = Vector3.zero;
        float strongestPull = 0f;
        bool captured = false;
        HeartLure strongestLure = null;
        HeartLure capturingLure = null;

        var lures = FindObjectsByType<HeartLure>(FindObjectsSortMode.None);
        foreach (var lure in lures)
        {
            Vector3 pull = lure.GetPull(transform.position, out bool thisCaptured);
            if (thisCaptured)
            {
                captured = true;
                capturingLure = lure;
            }

            if (pull.sqrMagnitude > strongestPull * strongestPull)
            {
                lurePull = pull;
                strongestPull = pull.magnitude;
                strongestLure = lure;
            }
        }

        IsMesmerized = captured;
        CurrentLure = captured ? capturingLure : (strongestPull > 0f ? strongestLure : null);
        CurrentPullStrength = strongestPull;

        if (captured)
        {
            currentVelocity = Vector3.zero;
            return;
        }

        Vector3 combinedDesired = desiredVelocity * willpower + lurePull;

        Vector3 velocity = Vector3.SmoothDamp(
            new Vector3(currentVelocity.x, 0, currentVelocity.z),
            combinedDesired,
            ref currentVelocity,
            smoothTime);

        transform.position += velocity * Time.deltaTime;
    }



    // Called by the player when it physically bumps into the companion.
    // The player is bigger/stronger, so it shoves the companion aside
    // instead of getting blocked; the push blends into the existing
    public void ApplyPush(Vector3 push)
    {
        currentVelocity += push;
    }


void OnEnable()
    {
        // start fresh whenever following resumes (e.g. after sitting or being put down)
        // otherwise it snaps using leftover velocity/state from before it was disabled.
        currentVelocity = Vector3.zero;
        isFollowing = false;
        IsMesmerized = false;
    }


void Start()
    {
        corruption = GetComponent<CompanionCorruption>();
    }
}
