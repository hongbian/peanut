using UnityEngine;

public class HazardZone : MonoBehaviour
{
    [SerializeField] private float damage = 1f;

    private void OnTriggerStay(Collider other)
    {
        var health = other.GetComponent<CompanionHealth>();
        if (health != null)
            health.TakeDamage(damage);
    }
}
