using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    private Vector3 lastCheckpointPosition;

    public Vector3 LastCheckpointPosition => lastCheckpointPosition;

    public bool HasCheckpoint { get; private set; }

    public void SetCheckpoint(Vector3 position)
    {
        lastCheckpointPosition = position;
        HasCheckpoint = true;

        Debug.Log(
            $"Checkpoint saved at: {lastCheckpointPosition}",
            this
        );
    }

    public void ClearCheckpoint()
    {
        lastCheckpointPosition = Vector3.zero;
        HasCheckpoint = false;
    }
}