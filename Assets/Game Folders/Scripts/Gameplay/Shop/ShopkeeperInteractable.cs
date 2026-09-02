using UnityEngine;

/// <summary>
/// Interaction untuk Penjaga Kedai.
/// Berinteraksi dengan NPC ini → ShopUI terbuka.
///
/// Tidak menangani Sanity recovery atau dialogue biasa.
/// Untuk dialogue biasa dengan NPC, gunakan DialogueInteractable.
///
/// Cara pakai:
///   1. Tambahkan ke GameObject Penjaga Kedai di scene.
///   2. Assign ShopUI di Inspector.
///   3. Pastikan Collider2D ada dan layer-nya masuk Interactable Layer.
/// </summary>
public class ShopkeeperInteractable : InteractableBase
{
    [Header("References")]
    [Tooltip("ShopUI yang akan dibuka saat interaksi. Wajib diisi.")]
    [SerializeField] private ShopUI shopUI;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private void Awake()
    {
        if (shopUI == null)
            shopUI = FindFirstObjectByType<ShopUI>();
    }

    public override bool CanInteract(GameObject interactor)
    {
        if (!base.CanInteract(interactor)) return false;

        if (shopUI == null)
        {
            Debug.LogWarning("[ShopkeeperInteractable] ShopUI tidak ditemukan.", this);
            return false;
        }

        // Tidak bisa buka shop jika sudah terbuka
        if (shopUI.IsOpen) return false;

        return true;
    }

    public override void Interact(GameObject interactor)
    {
        if (shopUI == null)
        {
            Debug.LogWarning("[ShopkeeperInteractable] ShopUI null saat Interact.", this);
            interactor.GetComponent<PlayerStateController>()?.ExitInteraction();
            return;
        }

        if (enableDebugLog) Debug.Log("[ShopkeeperInteractable] Membuka ShopUI.", this);

        // ShopUI menyimpan referensi interactor agar bisa ExitInteraction saat shop ditutup
        shopUI.Open(interactor);
    }
}
