using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Health Bar untuk menampilkan HP bos (6 segmen).
/// Subscribe ke EvilInaraBossController.OnHPChanged dan update visual.
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EvilInaraBossController bossController;

    [Header("UI — Segmented Bar")]
    [Tooltip("Array Image/Slider yang merepresentasikan setiap HP bos. Index 0 = HP pertama.")]
    [SerializeField] private Image[] hpSegments;
    [SerializeField] private Color activeSegmentColor = new Color(0.7f, 0.1f, 0.9f, 1f);    // Ungu gelap
    [SerializeField] private Color depletedSegmentColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Abu-abu pudar

    [Header("Boss Name UI")]
    [SerializeField] private TMPro.TextMeshProUGUI bossNameText;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private void Awake()
    {
        bossController ??= FindObjectOfType<EvilInaraBossController>();
    }

    private void OnEnable()
    {
        if (bossController != null)
        {
            bossController.OnHPChanged += UpdateBar;
            // Inisialisasi bar ke kondisi penuh
            UpdateBar(bossController.CurrentHP);
        }

        if (bossNameText != null)
            bossNameText.text = "Evil Inara";
    }

    private void OnDisable()
    {
        if (bossController != null)
            bossController.OnHPChanged -= UpdateBar;
    }

    private void UpdateBar(int currentHP)
    {
        Log($"Update HP Bar: {currentHP}");

        for (int i = 0; i < hpSegments.Length; i++)
        {
            if (hpSegments[i] == null) continue;

            // Segmen aktif jika index-nya lebih kecil dari HP saat ini
            // Contoh: HP=4, segmen 0-3 aktif, 4-5 depleted
            bool isActive = i < currentHP;
            hpSegments[i].color = isActive ? activeSegmentColor : depletedSegmentColor;
        }
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[BossHealthBarUI] {message}", this);
    }
}
