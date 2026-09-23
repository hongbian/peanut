using UnityEngine;

// The "state" channel to the gaze's "direction" channel: baseline warmth when calm,
// ramping toward a bright flare as a lure pulls harder, full flare while mesmerized,
// slow fade back down after rescue. Exposed publicly -- this will be the boss's
// heart-sensing input later, not just a visual, so it must stay readable by other systems.
public class CompanionGlow : MonoBehaviour
{
    [SerializeField] private float baseGlow = 0.3f;
    [SerializeField] private float mesmerizedGlow = 2.5f;
    [SerializeField] private float referenceMaxPull = 1.2f; // typical HeartLure.lureStrength -- normalizes the ramp
    [SerializeField] private float riseTime = 0.3f;         // seconds to climb to a higher target -- fast, matches gaze's early-warning feel
    [SerializeField] private float fadeOutTime = 2f;        // seconds to fade back down after rescue -- slow, deliberate
    [SerializeField] private Color glowColor = new Color(1f, 0.6f, 0.2f);
    [SerializeField] private Transform player;

    private Renderer rend;
    private CompanionFollow follow;
    private CompanionCorruption corruption;
    private CompanionCarry carry;
    private float currentIntensity;

    public float Intensity => currentIntensity;

void Start()
    {
        rend = GetComponent<Renderer>();
        follow = GetComponent<CompanionFollow>();
        corruption = GetComponent<CompanionCorruption>();
        if (player != null) carry = player.GetComponent<CompanionCarry>();
        rend.material.EnableKeyword("_EMISSION");
        currentIntensity = baseGlow;
    }

    void Update()
    {
        float target = ComputeTargetIntensity();

        // fast rise, slow fall -- flares up quickly, fades back down deliberately over fadeOutTime
        float rate = target > currentIntensity
            ? mesmerizedGlow / Mathf.Max(riseTime, 0.001f)
            : mesmerizedGlow / Mathf.Max(fadeOutTime, 0.001f);

        currentIntensity = Mathf.MoveTowards(currentIntensity, target, rate * Time.deltaTime);

        rend.material.SetColor("_EmissionColor", glowColor * currentIntensity);
    }

private float ComputeTargetIntensity()
    {
        float corruptionAmount = corruption != null ? corruption.Corruption : 0f;
        float baseline = Mathf.Lerp(baseGlow, 0f, corruptionAmount); // a corrupted heart is colder

        // carried = shielded from lures entirely; rescue is the moment the flare starts fading back to calm.
        // Checked before IsMesmerized because that flag freezes stale (true) the instant CompanionFollow
        // gets disabled by pickup -- same trap CompanionCorruption already guards against.
        bool isCarried = carry != null && carry.IsCarrying;
        if (isCarried || follow == null) return baseline;

        if (follow.IsMesmerized) return mesmerizedGlow; // acute flare -- not dimmed by corruption

        float pullRatio = Mathf.Clamp01(follow.CurrentPullStrength / referenceMaxPull);
        return Mathf.Lerp(baseline, mesmerizedGlow, pullRatio);
    }
}
