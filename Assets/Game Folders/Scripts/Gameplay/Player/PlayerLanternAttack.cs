using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLanternAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject lanternObject;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private PlayerCloakOfInvisibility cloak;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Attack")]
    [SerializeField] private float damageAmount = 1f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Lantern Visual — Sinkron Animasi")]
    [Tooltip(
        "Nama state animasi attack di Animator Controller.\n" +
        "Harus SAMA PERSIS dengan nama state di Animator (case-sensitive).\n" +
        "Contoh: \"Attack\", \"LanternAttack\", \"Player_Attack\""
    )]
    [SerializeField] private string attackStateName = "Attack";

    [Tooltip(
        "Layer Animator di mana state attack berada.\n" +
        "Biasanya 0 (Base Layer). Ubah jika attack ada di layer override."
    )]
    [SerializeField] private int animatorLayer = 0;

    [Tooltip(
        "Fallback: durasi lantern aktif (detik) jika Animator tidak ditemukan.\n" +
        "Sesuaikan dengan panjang animasi attack kamu."
    )]
    [SerializeField] private float lanternFallbackDuration = 0.5f;

    [Tooltip(
        "Delay (detik) sebelum lanternObject mulai muncul setelah attack dimulai.\n" +
        "Gunakan ini untuk sinkronisasi dengan animasi wind-up / anticipation frame.\n" +
        "Contoh: 0.1 = lantern muncul 0.1 detik setelah tombol attack ditekan.\n" +
        "Set 0 agar lantern langsung muncul tanpa delay."
    )]
    [SerializeField, Min(0f)] private float lanternActivationDelay = 0f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private bool showAttackGizmo = true;

    private IPlayerInput input;
    private float cooldownTimer;
    private float baseAttackRadius;
    private bool isAttacking;

    // Referensi Animator — diambil otomatis dari PlayerAnimator
    private Animator animator;
    private Coroutine lanternVisualCoroutine;
    // Hash untuk perbandingan state yang lebih efisien
    private int attackStateHash;

    private void Awake()
    {
        input = GetComponent<IPlayerInput>();

        if (input == null)
            Debug.LogError("[PlayerLanternAttack] IPlayerInput tidak ditemukan.", this);

        playerAnimator ??= GetComponent<PlayerAnimator>();
        cloak ??= GetComponent<PlayerCloakOfInvisibility>();
        playerMovement ??= GetComponent<PlayerMovement>();

        // Ambil Animator dari PlayerAnimator (atau langsung dari GameObject)
        if (playerAnimator != null)
            animator = playerAnimator.GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Pre-compute hash nama state agar tidak GC alloc setiap frame
        attackStateHash = Animator.StringToHash(attackStateName);

        if (lanternObject != null)
            lanternObject.SetActive(false);

        cooldownTimer = 0f;
        baseAttackRadius = attackRadius;
        isAttacking = false;
    }

    public void SetAttackRadiusBonus(float bonusRadius)
    {
        attackRadius = baseAttackRadius + bonusRadius;

        if (enableDebugLog)
            Debug.Log($"[PlayerLanternAttack] Attack radius: {baseAttackRadius:F2} + {bonusRadius:F2} = {attackRadius:F2}", this);
    }

    private void Update()
    {
        if (input == null) return;

        UpdateCooldown();
        // Catatan: UpdateLanternVisual() sengaja dihapus dari sini.
        // Lantern kini dikendalikan oleh coroutine ShowLanternForAttackAnimation()

        if (input.AttackPressed)
            TryAttack();
    }

    private void UpdateCooldown()
    {
        if (cooldownTimer <= 0f) return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer < 0f)
            cooldownTimer = 0f;
    }

    // UpdateLanternVisual() sudah TIDAK lagi dipakai (dihapus).
    // Visibilitas lantern sekarang 100% dikendalikan oleh ShowLanternForAttackAnimation().
    // Ini memastikan lantern muncul dan hilang SINKRON dengan animasi, bukan dengan input.

    private void TryAttack()
    {
        if (isAttacking)
            return;

        if (cooldownTimer > 0f)
        {
            if (enableDebugLog)
                Debug.Log($"[Lantern] Cooldown {cooldownTimer:F2}s", this);

            return;
        }

        isAttacking = true;

        if (playerAnimator != null)
        {
            playerAnimator.SetAttacking(true);
            playerAnimator.TriggerAttack();
        }

        if (playerMovement != null)
            playerMovement.SetMovementLocked(true);

        if (cloak != null && cloak.IsCloaked)
            cloak.ForceDeactivate();

        // Mulai coroutine: tampilkan lantern sinkron dengan animasi
        if (lanternVisualCoroutine != null)
            StopCoroutine(lanternVisualCoroutine);
        lanternVisualCoroutine = StartCoroutine(ShowLanternForAttackAnimation());

        FireLantern();

        cooldownTimer = Mathf.Max(0f, attackCooldown);
    }

    public void EndAttack()
    {
        isAttacking = false;

        if (playerAnimator != null)
            playerAnimator.SetAttacking(false);

        if (playerMovement != null)
            playerMovement.SetMovementLocked(false);

        // Pastikan lantern mati jika EndAttack dipanggil dari luar (misal: AnimationEvent)
        SetLanternActive(false);

        // Stop coroutine agar tidak ada konflik
        if (lanternVisualCoroutine != null)
        {
            StopCoroutine(lanternVisualCoroutine);
            lanternVisualCoroutine = null;
        }
    }

    // ─── Lantern Visual Coroutine ──────────────────────────────────────────

    /// <summary>
    /// Menyalakan lanternObject dan menunggunya selama animasi attack berjalan.
    /// Setelah animasi selesai, lanternObject dimatikan otomatis.
    ///
    /// CARA KERJA:
    ///   1. (Opsional) Tunggu lanternActivationDelay detik sebelum lantern muncul
    ///   2. Nyalakan lanternObject
    ///   3. Tunggu 1 frame agar Animator sempat transisi ke state attack
    ///   4. Selama state attack masih berjalan (via AnimatorStateInfo) → lantern tetap menyala
    ///   5. Begitu keluar dari state attack → matikan lantern
    ///   6. Fallback timer jika Animator tidak tersedia / state tidak ditemukan
    /// </summary>
    private IEnumerator ShowLanternForAttackAnimation()
    {
        // Terapkan delay sebelum lantern muncul (misal: saat animasi wind-up)
        if (lanternActivationDelay > 0f)
        {
            if (enableDebugLog)
                Debug.Log($"[Lantern Visual] Menunggu delay {lanternActivationDelay:F2}s...", this);

            yield return new WaitForSeconds(lanternActivationDelay);
        }

        // Nyalakan lantern setelah delay
        SetLanternActive(true);

        if (enableDebugLog)
            Debug.Log("[Lantern Visual] ON — menunggu animasi selesai", this);

        if (animator != null)
        {
            // Tunggu 1 frame agar TriggerAttack() sempat mengubah state Animator
            yield return null;

            // Tunggu sampai Animator benar-benar masuk state attack
            // (ada jeda transition, jadi polling selama maksimal 0.5 detik)
            float waitForStateTimeout = 0.5f;
            float waited = 0f;
            while (waited < waitForStateTimeout)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(animatorLayer);
                if (stateInfo.shortNameHash == attackStateHash)
                    break;

                waited += Time.deltaTime;
                yield return null;
            }

            if (enableDebugLog)
            {
                AnimatorStateInfo si = animator.GetCurrentAnimatorStateInfo(animatorLayer);
                bool found = si.shortNameHash == attackStateHash;
                Debug.Log($"[Lantern Visual] State '{attackStateName}' ditemukan: {found}", this);
            }

            // Selama masih di state attack, lantern tetap aktif
            while (true)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(animatorLayer);

                // Keluar dari loop jika sudah tidak di state attack
                if (stateInfo.shortNameHash != attackStateHash)
                    break;

                // Juga keluar jika animasi sudah di ujung (normalizedTime >= 1)
                // Ini handle kasus non-looping state
                if (!stateInfo.loop && stateInfo.normalizedTime >= 1f)
                    break;

                yield return null;
            }
        }
        else
        {
            // ─ Fallback jika tidak ada Animator ─
            // Gunakan timer berdasarkan lanternFallbackDuration
            if (enableDebugLog)
                Debug.LogWarning(
                    $"[Lantern Visual] Animator tidak ditemukan, pakai fallback timer ({lanternFallbackDuration}s)",
                    this
                );

            yield return new WaitForSeconds(lanternFallbackDuration);
        }

        // Animasi selesai → matikan lantern
        SetLanternActive(false);
        lanternVisualCoroutine = null;

        if (enableDebugLog)
            Debug.Log("[Lantern Visual] OFF — animasi selesai", this);
    }

    /// <summary>
    /// Helper untuk menyalakan/mematikan lanternObject dengan null-check.
    /// </summary>
    private void SetLanternActive(bool active)
    {
        if (lanternObject == null) return;
        if (lanternObject.activeSelf == active) return; // Hindari Set berulang
        lanternObject.SetActive(active);
    }

    private void FireLantern()
    {
        Vector2 attackOrigin = GetAttackOrigin();

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackOrigin, attackRadius, enemyLayer);

        if (enableDebugLog)
            Debug.Log($"[Lantern] Query → {hits.Length} collider(s) | Origin={attackOrigin} | Radius={attackRadius}", this);

        HashSet<EnemyLanternTarget> targets = new HashSet<EnemyLanternTarget>();
        int successfulHits = 0;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            if (enableDebugLog)
                Debug.Log($"[Lantern] Found → {hit.name} | Layer={LayerMask.LayerToName(hit.gameObject.layer)}", this);

            EnemyLanternTarget target = hit.GetComponentInParent<EnemyLanternTarget>();

            if (target == null)
            {
                // Cek apakah ini hitbox boss Evil Inara
                EvilInaraDamageReceiver bossTarget = hit.GetComponentInParent<EvilInaraDamageReceiver>();
                if (bossTarget != null && bossTarget.TryReceiveLanternHit(damageAmount))
                    successfulHits++;
                else if (enableDebugLog)
                    Debug.Log($"[Lantern] No Target → {hit.name}", this);

                continue;
            }

            if (!targets.Add(target))
                continue;

            if (target.TryReceiveLanternHit(damageAmount))
                successfulHits++;
        }

        if (enableDebugLog)
            Debug.Log($"[Lantern] Result → {successfulHits} hit", this);
    }

    private Vector2 GetAttackOrigin()
    {
        if (lanternObject != null)
            return lanternObject.transform.position;

        return transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showAttackGizmo) return;

        Vector2 origin = lanternObject != null ? lanternObject.transform.position : transform.position;
        Gizmos.DrawWireSphere(origin, attackRadius);
    }
}