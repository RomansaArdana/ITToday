using System.Collections.Generic;
using UnityEngine;

public class PlayerLanternAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject lanternObject;

    [Header("Attack")]
    [SerializeField] private float damageAmount = 1f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private bool showAttackGizmo = true;

    private IPlayerInput input;

    private float cooldownTimer;

    private void Awake()
    {
        input = GetComponent<IPlayerInput>();

        if (input == null)
        {
            Debug.LogError(
                "[PlayerLanternAttack] IPlayerInput tidak ditemukan.",
                this
            );
        }

        if (lanternObject != null)
        {
            lanternObject.SetActive(false);
        }

        cooldownTimer = 0f;
    }

    private void Update()
    {
        if (input == null)
        {
            return;
        }

        UpdateCooldown();
        UpdateLanternVisual();

        if (input.AttackPressed)
        {
            TryAttack();
        }
    }

    private void UpdateCooldown()
    {
        if (cooldownTimer <= 0f)
        {
            return;
        }

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer < 0f)
        {
            cooldownTimer = 0f;
        }
    }

    private void UpdateLanternVisual()
    {
        if (lanternObject == null)
        {
            return;
        }

        lanternObject.SetActive(
            input.AttackHeld
        );
    }

    private void TryAttack()
    {
        if (cooldownTimer > 0f)
        {
            if (enableDebugLog)
            {
                Debug.Log(
                    $"[Lantern] Cooldown {cooldownTimer:F2}s",
                    this
                );
            }

            return;
        }

        FireLantern();

        cooldownTimer =
            Mathf.Max(
                0f,
                attackCooldown
            );
    }

    private void FireLantern()
    {
        Vector2 attackOrigin =
            GetAttackOrigin();

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                attackOrigin,
                attackRadius,
                enemyLayer
            );

        if (enableDebugLog)
        {
            Debug.Log(
                $"[Lantern] Query → {hits.Length} collider(s) | " +
                $"Origin={attackOrigin} | " +
                $"Radius={attackRadius}",
                this
            );
        }

        HashSet<EnemyLanternTarget> targets =
            new HashSet<EnemyLanternTarget>();

        int successfulHits = 0;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            if (enableDebugLog)
            {
                Debug.Log(
                    $"[Lantern] Found → {hit.name} | Layer={LayerMask.LayerToName(hit.gameObject.layer)}",
                    this
                );
            }

            EnemyLanternTarget target =
                hit.GetComponentInParent<EnemyLanternTarget>();

            if (target == null)
            {
                if (enableDebugLog)
                {
                    Debug.Log(
                        $"[Lantern] No Target → {hit.name}",
                        this
                    );
                }

                continue;
            }

            if (!targets.Add(target))
            {
                continue;
            }

            if (target.TryReceiveLanternHit(
                    damageAmount))
            {
                successfulHits++;
            }
        }

        if (enableDebugLog)
        {
            Debug.Log(
                $"[Lantern] Result → {successfulHits} hit",
                this
            );
        }
    }

    private Vector2 GetAttackOrigin()
    {
        if (lanternObject != null)
        {
            return lanternObject.transform.position;
        }

        return transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showAttackGizmo)
        {
            return;
        }

        Vector2 origin =
            lanternObject != null
                ? lanternObject.transform.position
                : transform.position;

        Gizmos.DrawWireSphere(
            origin,
            attackRadius
        );
    }
}