using UnityEngine;

// Simple lever: player walks into it, it opens a gate elsewhere in the level.
public class Switch : MonoBehaviour
{
    [SerializeField] private GameObject gate;
    [SerializeField] private Color activatedColor = Color.green;

    private bool activated;

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        activated = true;
        if (gate != null) gate.SetActive(false);

        var rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = activatedColor;

        Debug.Log("Switch activated -- the way forward opens.");
    }
}
