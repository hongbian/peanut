using UnityEngine;
using UnityEngine.SceneManagement;

public class CompanionHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float invulnerabilityTime = 1f; // grace period between hits
    [SerializeField] private Renderer rend;
    [SerializeField] private Color hurtColor = Color.white;

    private float currentHealth;
    private float lastHitTime = -999f;

void Start()
    {
        currentHealth = maxHealth;
    }

public void TakeDamage(float amount)
    {
        if (Time.time - lastHitTime < invulnerabilityTime) return; // still in grace period
        lastHitTime = Time.time;

        currentHealth -= amount;
        Debug.Log($"Companion hurt! Health: {currentHealth}");

        StartCoroutine(Flash());
        var impulse = GetComponent<Unity.Cinemachine.CinemachineImpulseSource>();
        if (impulse != null) impulse.GenerateImpulse();

        if (currentHealth <= 0f)
            PlayerDies();
    }


System.Collections.IEnumerator Flash()
    {
        Color preFlashColor = rend.material.color; // captured fresh -- respects whatever corruption tint is currently applied
        rend.material.color = hurtColor;
        yield return new WaitForSeconds(0.12f);
        rend.material.color = preFlashColor;
    }


public void PlayerDies()
    {
        Debug.Log("The heart is gone. You die.");
        // For now: restart the scene. Later: death animation, fade, checkpoint.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
