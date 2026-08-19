using UnityEngine;

public class HideSpot : MonoBehaviour
{
    [SerializeField] private Transform hidePoint;

    private bool playerInside;

    public bool CanHide => playerInside;
    public Transform HidePoint => hidePoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = true;

        Debug.Log("Player entered Hide Spot.", this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = false;

        Debug.Log("Player exited Hide Spot.", this);
    }
}