using System.Collections;
using UnityEngine;

/// <summary>
/// Script untuk Tuas (Lever) dengan sistem TAHAN (HOLD) 'E':
/// 1. Player harus MENAHAN tombol 'E' untuk menarik tuas secara perlahan.
/// 2. Jika tombol 'E' dilepas sebelum tuas mentok, tuas akan kembali/mental ke posisi awal.
/// 3. Setelah tuas ditarik sampai MENTOK (full), tuas terkunci dan lemari otomatis terangkat ke atas.
/// </summary>
public class Lever : InteractableBase
{
    [Header("Hold & Pull Settings")]
    [Tooltip("Waktu (detik) yang dibutuhkan untuk menahan 'E' sampai tuas mentok penuh")]
    [SerializeField] private float pullDuration = 1.5f;
    [Tooltip("Kecepatan tuas kembali ke posisi awal jika tombol dilepas sebelum mentok")]
    [SerializeField] private float resetSpeed = 3f;

    [Header("Lever Visual Settings")]
    [Tooltip("Transform gagang tuas yang akan diputar")]
    [SerializeField] private Transform leverHandle;
    [Tooltip("Sudut rotasi Z tuas saat aktif/mentok (misal: -35 derajat ke kiri)")]
    [SerializeField] private float activeAngleZ = -35f;

    [Header("Pulley & Cabinet Settings")]
    [Tooltip("Transform lemari atau tumpukan rintangan yang akan diangkat")]
    [SerializeField] private Transform cabinetTransform;
    [Tooltip("Seberapa tinggi lemari diangkat ke atas (satuan Unity unit)")]
    [SerializeField] private float liftHeight = 3.5f;
    [Tooltip("Durasi waktu yang dibutuhkan lemari untuk terangkat penuh (detik)")]
    [SerializeField] private float liftDuration = 2.0f;
    [Tooltip("Kurva pergerakan pengangkatan lemari")]
    [SerializeField] private AnimationCurve liftCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isActivated = false;
    private bool isBeingPulled = false;
    private float pullProgress = 0f;
    private float initialAngleZ;
    private Vector3 initialCabinetPos;
    private PlayerInputReader currentInputReader;
    private PlayerStateController currentStateController;

    private void Awake()
    {
        if (leverHandle != null)
            initialAngleZ = leverHandle.localEulerAngles.z;

        // Normalisasi sudut jika di atas 180 derajat
        if (initialAngleZ > 180f)
            initialAngleZ -= 360f;

        if (cabinetTransform != null)
            initialCabinetPos = cabinetTransform.position;
    }

    private void Update()
    {
        if (isActivated)
            return;

        if (isBeingPulled)
        {
            // Cek apakah tombol E masih ditahan
            bool isHolding = currentInputReader != null &&
                             (currentInputReader.InteractHeld || currentInputReader.IsInteractHeld);

            if (isHolding)
            {
                // Tambah progress tarikan tuas
                pullProgress += Time.deltaTime / Mathf.Max(pullDuration, 0.1f);
                UpdateLeverRotation(pullProgress);

                // Jika sudah mentok penuh (100%)
                if (pullProgress >= 1f)
                {
                    CompleteLeverActivation();
                }
            }
            else
            {
                // Player melepas tombol sebelum mentok
                CancelPull();
            }
        }
        else if (pullProgress > 0f)
        {
            // Tuas perlahan kembali ke posisi awal (mental balik)
            pullProgress -= Time.deltaTime * resetSpeed;
            pullProgress = Mathf.Max(pullProgress, 0f);
            UpdateLeverRotation(pullProgress);
        }
    }

    public override void Interact(GameObject interactor)
    {
        if (isActivated || isBeingPulled)
            return;

        currentInputReader = interactor.GetComponent<PlayerInputReader>();
        currentStateController = interactor.GetComponent<PlayerStateController>();

        if (currentInputReader == null)
        {
            Debug.LogError("[Lever] PlayerInputReader tidak ditemukan pada " + interactor.name);
            return;
        }

        isBeingPulled = true;
    }

    private void CancelPull()
    {
        isBeingPulled = false;
        currentStateController?.ExitInteraction();
        currentInputReader = null;
        currentStateController = null;
    }

    private void CompleteLeverActivation()
    {
        isActivated = true;
        isBeingPulled = false;
        pullProgress = 1f;
        UpdateLeverRotation(1f);

        SetInteractionEnabled(false); // Kunci tuas agar tidak bisa diinteraksi lagi

        // Lepas state interaksi player agar player bebas bergerak kembali
        currentStateController?.ExitInteraction();
        currentInputReader = null;
        currentStateController = null;

        // Mulai pengangkatan lemari
        StartCoroutine(LiftCabinetRoutine());
    }

    private void UpdateLeverRotation(float progress)
    {
        if (leverHandle == null)
            return;

        float clampedProgress = Mathf.Clamp01(progress);
        float currentAngle = Mathf.Lerp(initialAngleZ, activeAngleZ, clampedProgress);
        leverHandle.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }

    private IEnumerator LiftCabinetRoutine()
    {
        // Delay singkat setelah tuas mentok
        yield return new WaitForSeconds(0.2f);

        if (cabinetTransform != null)
        {
            Vector3 startPos = cabinetTransform.position;
            Vector3 targetPos = initialCabinetPos + new Vector3(0, liftHeight, 0);

            float timer = 0f;
            while (timer < liftDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / liftDuration);
                float curvedProgress = liftCurve.Evaluate(progress);

                cabinetTransform.position = Vector3.Lerp(startPos, targetPos, curvedProgress);
                yield return null;
            }

            cabinetTransform.position = targetPos;
        }

        Debug.Log("[Lever] Tuas telah mentok! Lemari selesai diangkat.");
    }

    private void OnDrawGizmosSelected()
    {
        if (cabinetTransform != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 target = cabinetTransform.position + new Vector3(0, liftHeight, 0);
            Gizmos.DrawLine(cabinetTransform.position, target);
            Gizmos.DrawWireCube(target, Vector3.one * 0.5f);
        }
    }
}
