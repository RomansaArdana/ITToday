using UnityEngine;

/// <summary>
/// Interaction untuk kursi istirahat di Kedai.
/// Berinteraksi dengan kursi → restore Sanity penuh secara instan.
///
/// Tidak berhubungan dengan Shop atau TokenManager.
/// Terpisah dari ShopkeeperInteractable.
///
/// Cara pakai:
///   1. Tambahkan ke GameObject kursi di scene Kedai.
///   2. Pastikan Collider2D ada dan layer-nya masuk Interactable Layer.
///   3. SanityController akan di-find otomatis jika tidak di-assign.
/// </summary>
public class RestChairInteractable : InteractableBase
{
    [Header("References")]
    [Tooltip("SanityController milik Player. Auto-find jika kosong.")]
    [SerializeField] private SanityController sanityController;

    [Header("Settings")]
    [Tooltip("Jika true, RestoreFull. Jika false, hanya restore sebagian.")]
    [SerializeField] private bool restoreFullSanity = true;

    [Tooltip("Jumlah sanity yang dipulihkan jika restoreFullSanity = false.")]
    [SerializeField] private float partialRestoreAmount = 50f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private void Awake()
    {
        if (sanityController == null)
            sanityController = FindFirstObjectByType<SanityController>();
    }

    public override void Interact(GameObject interactor)
    {
        // Coba dapatkan SanityController dari interactor jika tidak di-assign
        if (sanityController == null)
            sanityController = interactor.GetComponent<SanityController>();

        if (sanityController == null)
        {
            Debug.LogWarning("[RestChairInteractable] SanityController tidak ditemukan.", this);
            // Bebaskan state interaksi agar player tidak terkunci
            interactor.GetComponent<PlayerStateController>()?.ExitInteraction();
            return;
        }

        if (restoreFullSanity)
        {
            sanityController.RestoreFullSanity();
            if (enableDebugLog) Debug.Log("[RestChairInteractable] Sanity dipulihkan penuh.", this);
        }
        else
        {
            sanityController.RecoverSanity(partialRestoreAmount);
            if (enableDebugLog) Debug.Log($"[RestChairInteractable] Sanity dipulihkan +{partialRestoreAmount:F1}.", this);
        }

        interactor.GetComponent<PlayerStateController>()?.ExitInteraction();
    }
}
