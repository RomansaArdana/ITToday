using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public bool IsDead { get; private set; }

    public void Die()
    {
        if (IsDead) return;

        IsDead = true;
    }

    public void Revive()
    {
        IsDead = false;
    }
}