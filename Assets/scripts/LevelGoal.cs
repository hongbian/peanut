using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    private bool reached;

    private void OnTriggerEnter(Collider other)
    {
        if (reached) return;
        if (!other.CompareTag("Player")) return;

        reached = true;
        Debug.Log("You made it. Vertical slice complete.");
    }
}
