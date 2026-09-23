using UnityEngine;

public class CompanionCarry : MonoBehaviour
{
    [SerializeField] private Transform companion;
    [SerializeField] private Transform carryAnchor;
    [SerializeField] private float pickupRange = 1.8f;
    [SerializeField] private KeyCode sitToggleKey = KeyCode.F;
    [SerializeField] private float restingHeight = 0.78f; // actual floor contact height (player's CharacterController center/height puts the pivot well above the feet)

    private CompanionFollow followScript;
    private Collider companionCollider;
    private bool isCarrying;

void Start()
    {
        followScript = companion.GetComponent<CompanionFollow>();
        companionCollider = companion.GetComponent<Collider>();
    }

void Update()
    {
        if (isCarrying)
        {
            if (Input.GetKeyDown(KeyCode.E)) PutDown();
            return;
        }

        bool nearCompanion = IsNearCompanion();

        if (Input.GetKeyDown(KeyCode.E) && nearCompanion)
            PickUp();
        else if (Input.GetKeyDown(sitToggleKey) && nearCompanion)
            followScript.SetSitting(!followScript.IsSitting); // toggle sit/follow in place -- lures still apply while sitting
    }


    private bool IsNearCompanion()
    {
        Vector3 toCompanion = companion.position - transform.position;
        toCompanion.y = 0f; // horizontal distance only -- player and companion rest at different heights
        return toCompanion.magnitude <= pickupRange;
    }


void PickUp()
    {
        isCarrying = true;
        followScript.enabled = false;              // stop the follow AI
        companionCollider.enabled = false;         // carried = shielded from hazards
        companion.SetParent(carryAnchor);          // attach to the anchor
        companion.localPosition = Vector3.zero;    // snap into the hands
        companion.localRotation = Quaternion.identity;
    }

void PutDown()
    {
        isCarrying = false;
        companion.SetParent(null);                 // release into the world

        // place it on the ground in front of the character
        Vector3 dropPos = transform.position + transform.forward * 0.6f;
        dropPos.y = restingHeight;
        companion.position = dropPos;

        companionCollider.enabled = true;          // exposed again once set down
        followScript.enabled = true;               // resume ticking (lures can still act on it)
        followScript.SetSitting(true);             // but don't chase the player -- stay here until picked up
    }

    

    // While carrying a corrupted heart, moving it is heavier -- penalty scales with corruption.
    public float GetCarrySpeedMultiplier()
    {
        if (!isCarrying) return 1f;
        var corruption = companion.GetComponent<CompanionCorruption>();
        return corruption != null ? Mathf.Lerp(1f, 0.5f, corruption.Corruption) : 1f;
    }
public bool IsCarrying => isCarrying;          // other systems will ask this later
}
