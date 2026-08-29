using UnityEngine;

public class WaterHazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerDeathHandler deathHandler = other.GetComponentInParent<PlayerDeathHandler>();
        if (deathHandler != null)
        {
            deathHandler.HandleDeath();
        }
    }
}
