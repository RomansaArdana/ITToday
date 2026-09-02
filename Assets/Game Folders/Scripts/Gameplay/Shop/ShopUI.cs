using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller untuk tampilan dan interaksi Shop di Kedai.
///
/// ShopUI hanya bertanggung jawab atas PRESENTASI:
/// - Menampilkan Token count dari TokenManager
/// - Menampilkan daftar upgrade dari UpgradeDataSO
/// - Menampilkan level, harga, dan state tombol
/// - Meneruskan klik Buy ke UpgradeManager.TryPurchase()
/// - Menutup shop dan mengembalikan player ke state normal
///
/// ShopUI TIDAK:
/// - Menyimpan Token
/// - Menghitung harga
/// - Mengubah Lantern/Sanity/Cloak langsung
/// - Menyimpan upgrade level
///
/// Setup di Unity Editor:
/// 1. Buat Canvas → Panel (ShopPanel) → assign ke shopPanel.
/// 2. Buat TextMeshProUGUI untuk token display → assign ke tokenText.
/// 3. Buat prefab UpgradeSlotUI dan assign ke upgradeSlotPrefab.
/// 4. Buat ScrollRect Content atau LayoutGroup → assign ke upgradeListParent.
/// 5. Buat Button Close → assign ke closeButton.
/// 6. Assign semua UpgradeDataSO asset ke availableUpgrades array.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Root panel ShopUI. Aktif/nonaktif untuk buka/tutup.")]
    [SerializeField] private GameObject shopPanel;

    [Tooltip("Text yang menampilkan jumlah Token saat ini.")]
    [SerializeField] private TMP_Text tokenText;

    [Tooltip("Tombol untuk menutup ShopUI.")]
    [SerializeField] private Button closeButton;

    [Tooltip("Parent transform untuk list upgrade slot (LayoutGroup disarankan).")]
    [SerializeField] private Transform upgradeListParent;

    [Tooltip("Prefab untuk setiap baris upgrade di list.")]
    [SerializeField] private UpgradeSlotUI upgradeSlotPrefab;

    [Header("Upgrade Data")]
    [Tooltip("Semua UpgradeDataSO yang tersedia di shop ini, sesuai urutan tampil.")]
    [SerializeField] private UpgradeDataSO[] availableUpgrades;

    // ─────────────────────────────────────────────────────────────────────────
    // State
    // ─────────────────────────────────────────────────────────────────────────

    private GameObject currentInteractor;
    private readonly List<UpgradeSlotUI> spawnedSlots = new();

    public bool IsOpen { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        if (TokenManager.Instance != null)
            TokenManager.Instance.OnTokensChanged += OnTokensChanged;

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgradePurchased += OnUpgradePurchased;
    }

    private void OnDisable()
    {
        if (TokenManager.Instance != null)
            TokenManager.Instance.OnTokensChanged -= OnTokensChanged;

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgradePurchased -= OnUpgradePurchased;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Open / Close
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Buka ShopUI. Dipanggil oleh ShopkeeperInteractable.
    /// </summary>
    public void Open(GameObject interactor)
    {
        if (IsOpen) return;

        currentInteractor = interactor;
        IsOpen = true;

        if (shopPanel != null) shopPanel.SetActive(true);

        RefreshTokenDisplay();
        BuildUpgradeList();

        Debug.Log("[ShopUI] Shop dibuka.", this);
    }

    /// <summary>
    /// Tutup ShopUI dan kembalikan player ke state normal.
    /// </summary>
    public void Close()
    {
        if (!IsOpen) return;

        IsOpen = false;

        if (shopPanel != null) shopPanel.SetActive(false);

        // Kembalikan player ke state Idle setelah menutup shop
        if (currentInteractor != null)
            currentInteractor.GetComponent<PlayerStateController>()?.ExitInteraction();

        currentInteractor = null;

        Debug.Log("[ShopUI] Shop ditutup.", this);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Token Display
    // ─────────────────────────────────────────────────────────────────────────

    private void RefreshTokenDisplay()
    {
        if (tokenText == null) return;
        int tokens = TokenManager.Instance != null ? TokenManager.Instance.CurrentTokens : 0;
        tokenText.text = $"Token: {tokens}";
    }

    private void OnTokensChanged(int newAmount)
    {
        if (!IsOpen) return;
        RefreshTokenDisplay();
        RefreshAllSlots();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Upgrade List
    // ─────────────────────────────────────────────────────────────────────────

    private void BuildUpgradeList()
    {
        // Hapus slot lama
        foreach (UpgradeSlotUI slot in spawnedSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        spawnedSlots.Clear();

        if (upgradeSlotPrefab == null || upgradeListParent == null) return;
        if (availableUpgrades == null) return;

        foreach (UpgradeDataSO data in availableUpgrades)
        {
            if (data == null) continue;

            UpgradeSlotUI slot = Instantiate(upgradeSlotPrefab, upgradeListParent);
            slot.Setup(data, OnBuyClicked);
            slot.Refresh();
            spawnedSlots.Add(slot);
        }
    }

    private void RefreshAllSlots()
    {
        foreach (UpgradeSlotUI slot in spawnedSlots)
        {
            if (slot != null) slot.Refresh();
        }
    }

    private void OnUpgradePurchased(UpgradeType type)
    {
        if (!IsOpen) return;
        RefreshTokenDisplay();
        RefreshAllSlots();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Buy Handler
    // ─────────────────────────────────────────────────────────────────────────

    private void OnBuyClicked(UpgradeDataSO data)
    {
        if (data == null) return;
        if (UpgradeManager.Instance == null)
        {
            Debug.LogWarning("[ShopUI] UpgradeManager tidak ditemukan.", this);
            return;
        }

        bool success = UpgradeManager.Instance.TryPurchase(data);

        if (!success)
        {
            Debug.Log($"[ShopUI] Pembelian [{data.DisplayName}] gagal.", this);
        }
        // Refresh dipanggil via OnUpgradePurchased event jika sukses
    }
}
