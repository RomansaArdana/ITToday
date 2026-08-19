using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private CheckpointManager checkpointManager;
    [SerializeField] private Transform spawnPoint;

    private void Awake()
    {
        if (checkpointManager == null)
        {
            checkpointManager = FindFirstObjectByType<CheckpointManager>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (checkpointManager == null)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        Vector3 checkpointPosition = spawnPoint != null
            ? spawnPoint.position
            : transform.position;

        checkpointManager.SetCheckpoint(checkpointPosition);
    }
}