using UnityEngine;

public class Oasis : MonoBehaviour
{
    [SerializeField] private SanityController sanityController;

    public bool IsPlayerInside { get; private set; }

    private void Awake()
    {
        if (sanityController == null)
        {
            sanityController = FindFirstObjectByType<SanityController>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        IsPlayerInside = true;

        if (sanityController != null)
        {
            sanityController.SetRecoveryActive(true);
        }

        Debug.Log(
            "Player entered Oasis. Sanity recovery active.",
            this
        );
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        IsPlayerInside = false;

        if (sanityController != null)
        {
            sanityController.StopSanityChange();
        }

        Debug.Log(
            "Player exited Oasis. Sanity recovery stopped.",
            this
        );
    }
}