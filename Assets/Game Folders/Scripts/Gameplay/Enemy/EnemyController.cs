using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyStatsSO stats;
    [SerializeField] private Transform playerTarget;

    public EnemyStatsSO Stats => stats;
    public Transform PlayerTarget => playerTarget;

    public Vector2 FacingDirection => transform.right;

    public bool HasTarget => playerTarget != null;

    private void Awake()
    {
        if (playerTarget == null)
        {
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        if (stats == null)
        {
            Debug.LogError(
                "EnemyController: EnemyStatsSO belum di-assign.",
                this
            );
        }

        if (playerTarget == null)
        {
            Debug.LogWarning(
                "EnemyController: Player target tidak ditemukan.",
                this
            );
        }
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        direction.Normalize();

        float angle =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0f, 0f, angle);
    }
}