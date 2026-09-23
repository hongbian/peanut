using UnityEngine;

// A mesmerized heart slowly becomes doll-like: its color fades, eyes dull,
// and movement slows.
public class CompanionCorruption : MonoBehaviour
{
    [SerializeField] private float gainRate = 0.04f;  
    [SerializeField] private float recoveryRate = 0.01f; 
    [SerializeField] private Transform player;
    [SerializeField] private Color porcelainColor = new Color(0.85f, 0.83f, 0.8f, 1f);
    [SerializeField] private Transform eyeL;
    [SerializeField] private Transform eyeR;

    private CompanionFollow follow;
    private CompanionCarry carry;
    private CompanionHealth health;
    private Renderer rend;
    private Color baseColor;
    private Vector3 eyeBaseScale;
    private bool hasDied;

    public float Corruption { get; private set; }

    void Start()
    {
        follow = GetComponent<CompanionFollow>();
        health = GetComponent<CompanionHealth>();
        rend = GetComponent<Renderer>();
        baseColor = rend.material.color;
        if (eyeL != null) eyeBaseScale = eyeL.localScale; 
        if (player != null) carry = player.GetComponent<CompanionCarry>();
    }

    void Update()
    {
        bool isCarried = carry != null && carry.IsCarrying;

        if (isCarried)
        {
            Corruption -= recoveryRate * Time.deltaTime;
        }
        else if (follow != null && follow.IsMesmerized)
        {
            Corruption += gainRate * Time.deltaTime;
        }
        else if (IsCloseToPlayer())
        {
            Corruption -= recoveryRate * Time.deltaTime;
        }

        Corruption = Mathf.Clamp01(Corruption);

        ApplyVisuals();

        if (Corruption >= 1f && !hasDied)
        {
            hasDied = true;
            if (health != null) health.PlayerDies();
        }
    }

    private bool IsCloseToPlayer()
    {
        if (player == null || follow == null) return false;
        Vector3 diff = player.position - transform.position;
        diff.y = 0f;
        return diff.magnitude <= follow.FollowDistance;
    }

    private void ApplyVisuals()
    {
        if (rend != null)
            rend.material.color = Color.Lerp(baseColor, porcelainColor, Corruption);

        float eyeScale = Mathf.Lerp(1f, 0.35f, Corruption);
        if (eyeL != null) eyeL.localScale = eyeBaseScale * eyeScale;
        if (eyeR != null) eyeR.localScale = eyeBaseScale * eyeScale;
    }
}
