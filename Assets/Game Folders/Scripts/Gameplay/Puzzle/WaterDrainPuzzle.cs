using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Puzzle 4 "Katup Air Kenangan"
/// -----------------------------------------------
/// Alur:
/// 1. Jalan tertutup perabotan yang mengapung di air.
/// 2. Player lompat ke platform agar tidak tersedot saat air surut.
/// 3. Player pake Lentera (F / Attack) → memurnikan sulur biru → katup terbuka.
/// 4-5. Air surut bertahap (drainingWater turun).
/// 6. Perabotan kehilangan penyangga air → jatuh ke bawah (collider diaktifkan, gravity on).
///
/// Setup Hierarchy:
/// ─ WaterDrainPuzzle (this script)
///   ├── ValveTransform        → Transform katup/roda yang akan berputar
///   ├── BlueVineRenderer      → SpriteRenderer sulur biru (akan fade out)
///   ├── DrainingWater         → Transform sprite air (akan turun)
///   ├── FurnitureParent       → Parent semua perabotan (Rigidbody2D tiap anak)
///   └── [optional] ActivationZone → radius deteksi lentera
/// </summary>
public class WaterDrainPuzzle : MonoBehaviour
{
    // ── References ────────────────────────────────────────────────────────────
    [Header("References")]
    [Tooltip("Transform katup/roda yang akan berputar saat terbuka")]
    [SerializeField] private Transform valveTransform;

    [Tooltip("SpriteRenderer sulur biru pada katup (akan di-fade out)")]
    [SerializeField] private SpriteRenderer blueVineRenderer;

    [Tooltip("Transform sprite air yang akan turun (bisa GameObject dengan SpriteRenderer)")]
    [SerializeField] private Transform drainingWater;

    [Tooltip("Parent GameObject yang berisi semua perabotan. Tiap anak harus punya Rigidbody2D (Kinematic saat awal).")]
    [SerializeField] private Transform furnitureParent;

    // ── Activation ────────────────────────────────────────────────────────────
    [Header("Activation (AOE Lentera)")]
    [Tooltip("Radius jangkauan AOE lentera dari posisi player ke katup")]
    [SerializeField] private float activationRadius = 8f;

    // ── Water Drain ───────────────────────────────────────────────────────────
    [Header("Water Drain")]
    [Tooltip("Total jarak air turun ke bawah (Unity units)")]
    [SerializeField] private float waterDrainDistanceY = 4f;

    [Tooltip("Durasi total air surut (detik)")]
    [SerializeField] private float waterDrainDuration = 3f;

    [Tooltip("Jumlah 'tahap' surutnya air (sesuai panel 4-5 storyboard)")]
    [SerializeField] private int drainStages = 3;

    // ── Furniture Fall ───────────────────────────────────────────────
    [Header("Furniture Fall")]
    [Tooltip("Delay setelah air HABIS sebelum perabotan mulai jatuh")]
    [SerializeField] private float furnitureFallDelay = 0.3f;

    [Tooltip("Gravity scale tiap furniture saat jatuh")]
    [SerializeField] private float furnitureGravityScale = 2.5f;

    [Tooltip("Delay antar tiap furniture mulai jatuh (urutan, bukan acak)")]
    [SerializeField] private float furnitureStaggerDelay = 0.12f;

    [Tooltip("Dorongan horizontal kecil agar terlihat natural (alternating kiri-kanan)")]
    [SerializeField] private float furnitureNudgeX = 0.4f;

    [Tooltip("Sudut target furniture saat sudah tiduran (pijakan player). 85 = hampir horizontal.")]
    [SerializeField] private float furnitureLieAngle = 85f;

    [Tooltip("Durasi animasi furniture berputar sampai tiduran (detik)")]
    [SerializeField] private float furnitureToppleDuration = 0.7f;

    [Tooltip("Seberapa jauh furniture ikut turun bersama air (biasanya sama dengan waterDrainDistanceY)")]
    [SerializeField] private float furnitureFollowDistance = 4f;

    // ── Valve Spin ────────────────────────────────────────────────────────────
    [Header("Valve Spin")]
    [Tooltip("Berapa putaran penuh katup saat terbuka")]
    [SerializeField] private float valveSpinRevolutions = 2f;

    [Tooltip("Durasi animasi putaran katup (detik)")]
    [SerializeField] private float valveSpinDuration = 0.6f;

    // ── Internal State ───────────────────────────────────────────────
    private bool isActivated = false;
    private PlayerInputReader playerInputReader;
    private Transform playerTransform;

    // ══════════════════════════════════════════════════════
    private void Start()
    {
        FindPlayer();
        InitFurniture(); // Kunci furniture agar tidak jatuh sebelum waktunya
    }

    /// <summary>
    /// Paksa semua furniture ke Kinematic dari awal
    /// sehingga tidak jatuh saat air mulai turun.
    /// </summary>
    private void InitFurniture()
    {
        if (furnitureParent == null) return;

        foreach (Rigidbody2D rb in furnitureParent.GetComponentsInChildren<Rigidbody2D>())
        {
            rb.bodyType     = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.linearVelocity   = Vector2.zero;
            rb.angularVelocity  = 0f;
        }
    }

    private void Update()
    {
        if (isActivated) return;

        if (IsFPressed() && IsPlayerInRange())
        {
            ActivateDrain();
        }
    }

    // ── Input Detection ───────────────────────────────────────────────────────

    private bool IsFPressed()
    {
        // Cek via PlayerInputReader (AttackPressed = tombol F/Lentera)
        if (playerInputReader != null && playerInputReader.AttackPressed)
            return true;

        // Fallback: keyboard langsung
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            return true;

        return false;
    }

    private bool IsPlayerInRange()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return true; // Jika tidak ada player, langsung aktif
        }

        Vector3 origin = valveTransform != null ? valveTransform.position : transform.position;
        return Vector2.Distance(playerTransform.position, origin) <= activationRadius;
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            PlayerMovement pm = Object.FindFirstObjectByType<PlayerMovement>();
            if (pm != null) playerObj = pm.gameObject;
        }

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerInputReader = playerObj.GetComponent<PlayerInputReader>();
        }
    }

    // ── Activation Entry ──────────────────────────────────────────────────────

    public void ActivateDrain()
    {
        if (isActivated) return;
        isActivated = true;
        StartCoroutine(PuzzleSequence());
    }

    // ══════════════════════════════════════════════════════════════════════════
    // MAIN SEQUENCE
    // ══════════════════════════════════════════════════════════════════════════

    private IEnumerator PuzzleSequence()
    {
        yield return StartCoroutine(FadeOutBlueVine());
        yield return StartCoroutine(SpinValve());
        yield return StartCoroutine(DrainWaterStaged());

        yield return new WaitForSeconds(1.0f);
        LockAllFurniturePermanently();
    }

    private void LockAllFurniturePermanently()
    {
        if (furnitureParent == null) return;

        foreach (Rigidbody2D rb in furnitureParent.GetComponentsInChildren<Rigidbody2D>())
        {
            if (rb == null) continue;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private IEnumerator FadeOutBlueVine()
    {
        if (blueVineRenderer == null) yield break;

        float elapsed = 0f;
        float duration = 0.4f;
        Color startColor = blueVineRenderer.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            blueVineRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        blueVineRenderer.gameObject.SetActive(false);
    }

    private IEnumerator SpinValve()
    {
        if (valveTransform == null) yield break;

        float elapsed = 0f;
        float totalAngle = 360f * valveSpinRevolutions;
        Quaternion startRot = valveTransform.rotation;

        while (elapsed < valveSpinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / valveSpinDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            valveTransform.rotation = startRot * Quaternion.Euler(0f, 0f, -totalAngle * eased);
            yield return null;
        }
    }

    private IEnumerator DrainWaterStaged()
    {
        if (drainingWater == null) yield break;

        Rigidbody2D[] rbs = furnitureParent != null
            ? furnitureParent.GetComponentsInChildren<Rigidbody2D>()
            : new Rigidbody2D[0];

        float stageDistance = waterDrainDistanceY / drainStages;
        float stageDuration = waterDrainDuration / drainStages;

        for (int stage = 0; stage < drainStages; stage++)
        {
            if (stage < rbs.Length && rbs[stage] != null)
            {
                StartCoroutine(DropAndLieSingleFurniture(rbs[stage]));
            }

            Vector3 stageStart = drainingWater.position;
            Vector3 stageEnd = stageStart + new Vector3(0f, -stageDistance, 0f);

            float elapsed = 0f;
            while (elapsed < stageDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / stageDuration));
                drainingWater.position = Vector3.Lerp(stageStart, stageEnd, t);
                yield return null;
            }

            drainingWater.position = stageEnd;

            if (stage < drainStages - 1)
                yield return new WaitForSeconds(0.2f);
        }

        drainingWater.gameObject.SetActive(false);
    }

    private IEnumerator DropAndLieSingleFurniture(Rigidbody2D rb)
    {
        if (rb == null) yield break;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = furnitureGravityScale;

        rb.linearVelocity = new Vector2(furnitureNudgeX, 0f);

        Quaternion startRot = rb.transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, -furnitureLieAngle);

        float elapsed = 0f;
        while (elapsed < furnitureToppleDuration)
        {
            if (rb == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / furnitureToppleDuration));
            rb.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        if (rb != null)
        {
            rb.transform.rotation = targetRot;
        }
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        // Radius aktivasi lentera
        Vector3 origin = valveTransform != null ? valveTransform.position : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, activationRadius);

        // Preview air turun
        if (drainingWater != null)
        {
            Gizmos.color = Color.blue;
            Vector3 waterEnd = drainingWater.position + new Vector3(0f, -waterDrainDistanceY, 0f);
            Gizmos.DrawLine(drainingWater.position, waterEnd);
            Gizmos.DrawWireSphere(waterEnd, 0.25f);
        }

        // Preview perabotan jatuh
        if (furnitureParent != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(furnitureParent.position, Vector3.one * 0.5f);
        }
    }
}
