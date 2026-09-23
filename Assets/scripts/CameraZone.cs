using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [SerializeField] private GameObject zoneCamera;   // camera for THIS zone
    [SerializeField] private GameObject defaultCamera; // camera to return to

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        zoneCamera.SetActive(true);
        defaultCamera.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        defaultCamera.SetActive(true);
        zoneCamera.SetActive(false);
    }
}