using UnityEngine;

public class EnemyHealthDebug : MonoBehaviour
{
    [SerializeField] private EnemyHealth health;

    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<EnemyHealth>();
        }
    }

    private void Update()
    {
        if (health == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            health.TakeDamage(1f);
        }
    }
}