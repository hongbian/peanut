using UnityEngine;

public class CompanionCarry : MonoBehaviour
{
    [SerializeField] private Transform companion;
    [SerializeField] private Transform carryAnchor;
    [SerializeField] private float pickupRange = 1.8f;
    [SerializeField] private KeyCode sitToggleKey = KeyCode.F;
    [SerializeField] private float restingHeight = 0.78f; 

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
            followScript.SetSitting(!followScript.IsSitting);
    }


    private bool IsNearCompanion()
    {
        Vector3 toCompanion = companion.position - transform.position;
        toCompanion.y = 0f;
        return toCompanion.magnitude <= pickupRange;
    }


void PickUp()
    {
        isCarrying = true;
        followScript.enabled = false;            
        companionCollider.enabled = false;       
        companion.SetParent(carryAnchor);       
        companion.localPosition = Vector3.zero;  
        companion.localRotation = Quaternion.identity;
    }

void PutDown()
    {
        isCarrying = false;
        companion.SetParent(null); 

        Vector3 dropPos = transform.position + transform.forward * 0.6f;
        dropPos.y = restingHeight;
        companion.position = dropPos;

        companionCollider.enabled = true; 
        followScript.enabled = true;              
        followScript.SetSitting(true);       
    }

    

    public float GetCarrySpeedMultiplier()
    {
        if (!isCarrying) return 1f;
        var corruption = companion.GetComponent<CompanionCorruption>();
        return corruption != null ? Mathf.Lerp(1f, 0.5f, corruption.Corruption) : 1f;
    }
public bool IsCarrying => isCarrying;          // other systems will ask this later
}
