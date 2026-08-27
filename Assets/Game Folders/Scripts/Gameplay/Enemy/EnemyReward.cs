using UnityEngine;

[RequireComponent(typeof(EnemyPurification))]
public class EnemyReward : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyPurification purification;
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private SanityController sanityController;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private bool rewardGranted;

    private void Awake()
    {
        purification ??=
            GetComponent<EnemyPurification>();

        enemyController ??=
            GetComponent<EnemyController>();

        CacheSanityController();
    }

    private void OnEnable()
    {
        if (purification != null)
        {
            purification.OnPurified += HandlePurified;
        }
    }

    private void OnDisable()
    {
        if (purification != null)
        {
            purification.OnPurified -= HandlePurified;
        }
    }

    private void CacheSanityController()
    {
        if (sanityController != null)
        {
            return;
        }

        if (enemyController == null ||
            !enemyController.HasTarget)
        {
            return;
        }

        sanityController =
            enemyController.PlayerTarget
                .GetComponent<SanityController>();
    }

    private void HandlePurified()
    {
        if (rewardGranted)
        {
            return;
        }

        rewardGranted = true;

        CacheSanityController();

        if (sanityController == null)
        {
            Debug.LogWarning(
                "[EnemyReward] SanityController tidak ditemukan.",
                this
            );

            return;
        }

        if (enemyController == null ||
            enemyController.Stats == null)
        {
            Debug.LogWarning(
                "[EnemyReward] EnemyStatsSO tidak ditemukan.",
                this
            );

            return;
        }

        float reward =
            enemyController.Stats
                .PurificationSanityReward;

        if (reward <= 0f)
        {
            return;
        }

        float previousSanity =
            sanityController.CurrentSanity;

        sanityController.RecoverSanity(reward);

        float currentSanity =
            sanityController.CurrentSanity;

        if (enableDebugLog)
        {
            Debug.Log(
                $"[EnemyReward] " +
                $"Purification reward: +{reward:F1} Sanity | " +
                $"{previousSanity:F1} → {currentSanity:F1}",
                this
            );
        }
    }
}