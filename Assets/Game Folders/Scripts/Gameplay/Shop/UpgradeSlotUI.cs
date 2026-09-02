using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Satu baris/slot di daftar upgrade ShopUI.
/// Menampilkan: nama, deskripsi, level sekarang, harga, dan tombol Buy.
/// State tombol dikontrol oleh PurchaseState dari UpgradeManager.
///
/// Cara pakai:
/// 1. Buat prefab dengan layout: NameText, DescText, LevelText, PriceText, BuyButton, StateText.
/// 2. Assign semua referensi di Inspector prefab.
/// 3. ShopUI.BuildUpgradeList() akan instantiate dan memanggil Setup() + Refresh().
/// </summary>
public class UpgradeSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private Button buyButton;

    // ─────────────────────────────────────────────────────────────────────────
    // State
    // ─────────────────────────────────────────────────────────────────────────

    private UpgradeDataSO data;
    private Action<UpgradeDataSO> onBuyClicked;

    // ─────────────────────────────────────────────────────────────────────────
    // Setup
    // ─────────────────────────────────────────────────────────────────────────

    public void Setup(UpgradeDataSO upgradeData, Action<UpgradeDataSO> buyCallback)
    {
        data = upgradeData;
        onBuyClicked = buyCallback;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyPressed);
        }

        if (nameText != null && data != null)
            nameText.text = data.DisplayName;

        if (descriptionText != null && data != null)
            descriptionText.text = data.Description;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Refresh (dipanggil setiap kali state berubah)
    // ─────────────────────────────────────────────────────────────────────────

    public void Refresh()
    {
        if (data == null || UpgradeManager.Instance == null) return;

        int currentLevel = UpgradeManager.Instance.GetLevel(data.UpgradeType);
        UpgradeManager.PurchaseState state = UpgradeManager.Instance.EvaluatePurchaseState(data);

        // Level display
        if (levelText != null)
            levelText.text = $"Level {currentLevel} / {data.MaxLevel}";

        // Price display
        int price = data.GetPriceForNextLevel(currentLevel);
        if (priceText != null)
            priceText.text = price >= 0 ? $"{price} Token" : "—";

        // State label dan warna
        if (stateText != null)
        {
            switch (state)
            {
                case UpgradeManager.PurchaseState.Affordable:
                    stateText.text = "Beli";
                    stateText.color = Color.green;
                    break;
                case UpgradeManager.PurchaseState.InsufficientTokens:
                    stateText.text = "Token Kurang";
                    stateText.color = Color.yellow;
                    break;
                case UpgradeManager.PurchaseState.Locked:
                    stateText.text = "Terkunci";
                    stateText.color = Color.red;
                    break;
                case UpgradeManager.PurchaseState.MaxLevel:
                    stateText.text = "Maksimal";
                    stateText.color = Color.gray;
                    break;
                default:
                    stateText.text = "";
                    break;
            }
        }

        // Aktif/nonaktifkan tombol Buy
        if (buyButton != null)
            buyButton.interactable = state == UpgradeManager.PurchaseState.Affordable;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Button Handler
    // ─────────────────────────────────────────────────────────────────────────

    private void OnBuyPressed()
    {
        onBuyClicked?.Invoke(data);
    }
}
