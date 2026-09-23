using UnityEngine;

// Maps the heart's state to its glow: warm when calm, brighter as the lure strengthens,
// fully lit when mesmerized, then slowly fading after rescue. Kept public for boss sensing.
public class CompanionGlow : MonoBehaviour
{
    [SerializeField] private float baseGlow = 0.3f;
    [SerializeField] private float mesmerizedGlow = 2.5f;
    [SerializeField] private float referenceMaxPull = 1.2f; 
    [SerializeField] private float riseTime = 0.3f;     
    [SerializeField] private float fadeOutTime = 2f;        
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

        float rate = target > currentIntensity
            ? mesmerizedGlow / Mathf.Max(riseTime, 0.001f)
            : mesmerizedGlow / Mathf.Max(fadeOutTime, 0.001f);

        currentIntensity = Mathf.MoveTowards(currentIntensity, target, rate * Time.deltaTime);

        rend.material.SetColor("_EmissionColor", glowColor * currentIntensity);
    }

private float ComputeTargetIntensity()
    {
        float corruptionAmount = corruption != null ? corruption.Corruption : 0f;
        float baseline = Mathf.Lerp(baseGlow, 0f, corruptionAmount); 


        bool isCarried = carry != null && carry.IsCarrying;
        if (isCarried || follow == null) return baseline;

        if (follow.IsMesmerized) return mesmerizedGlow;

        float pullRatio = Mathf.Clamp01(follow.CurrentPullStrength / referenceMaxPull);
        return Mathf.Lerp(baseline, mesmerizedGlow, pullRatio);
    }
}
