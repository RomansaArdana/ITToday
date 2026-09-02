using UnityEngine;

/// <summary>
/// ScriptableObject konfigurasi untuk satu jenis upgrade di Kedai.
/// Satu asset = satu upgrade (misal: "Lantern AOE", "Max Sanity", "Cloak Duration").
///
/// Semua nilai balancing dikonfigurasi di sini — tidak ada angka hardcode di kode.
/// Designer cukup mengubah nilai di Inspector tanpa menyentuh script.
///
/// Struktur harga & nilai per level menggunakan array agar tiap level bisa berbeda.
/// Index 0 = Level 1 (harga untuk upgrade dari Level 0 ke Level 1).
/// </summary>
[CreateAssetMenu(fileName = "Upgrade_New", menuName = "The Day After/Shop/Upgrade Data")]
public class UpgradeDataSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Tipe upgrade ini. Digunakan oleh UpgradeManager sebagai key — harus unik per tipe.")]
    [SerializeField] private UpgradeType upgradeType;

    [Tooltip("Nama yang ditampilkan di ShopUI.")]
    [SerializeField] private string displayName = "Upgrade";

    [TextArea(2, 4)]
    [Tooltip("Deskripsi singkat yang ditampilkan di ShopUI.")]
    [SerializeField] private string description = "";

    [Header("Levels")]
    [Tooltip("Jumlah level maksimum upgrade ini (tidak termasuk Level 0 / unowned).")]
    [SerializeField] private int maxLevel = 3;

    [Tooltip("Harga Token per level. Index 0 = harga upgrade ke Level 1, dst.\n" +
             "Jumlah elemen harus sama dengan maxLevel.")]
    [SerializeField] private int[] pricePerLevel = { 5, 8, 12 };

    [Tooltip("Nilai gameplay yang didapat per level. Index 0 = nilai di Level 1.\n" +
             "Untuk LanternAOE: radius tambahan. Untuk MaxSanity: HP tambahan. Untuk CloakDuration: detik tambahan.")]
    [SerializeField] private float[] valuePerLevel = { 0.5f, 1.0f, 1.5f };

    [Header("Prerequisite")]
    [Tooltip("Jika true, upgrade ini membutuhkan upgrade lain mencapai level tertentu terlebih dahulu.")]
    [SerializeField] private bool hasPrerequisite = false;

    [Tooltip("Tipe upgrade yang harus menjadi prerequisite.")]
    [SerializeField] private UpgradeType prerequisiteType = UpgradeType.LanternAOE;

    [Tooltip("Level minimum upgrade prerequisite yang harus sudah dimiliki.")]
    [SerializeField] private int prerequisiteLevel = 1;

    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    public UpgradeType UpgradeType => upgradeType;
    public string DisplayName => displayName;
    public string Description => description;
    public int MaxLevel => maxLevel;
    public bool HasPrerequisite => hasPrerequisite;
    public UpgradeType PrerequisiteType => prerequisiteType;
    public int PrerequisiteLevel => prerequisiteLevel;

    // ─────────────────────────────────────────────────────────────────────────
    // Level Data Access
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Harga Token untuk upgrade dari <paramref name="currentLevel"/> ke level berikutnya.
    /// Mengembalikan -1 jika currentLevel sudah maksimum atau data tidak valid.
    /// </summary>
    public int GetPriceForNextLevel(int currentLevel)
    {
        int targetIndex = currentLevel; // Level 0→1 = index 0, Level 1→2 = index 1, dst
        if (pricePerLevel == null || targetIndex < 0 || targetIndex >= pricePerLevel.Length)
            return -1;
        return pricePerLevel[targetIndex];
    }

    /// <summary>
    /// Nilai gameplay kumulatif pada level tertentu (bukan delta per level).
    /// Level 0 = 0 (tidak ada nilai). Level >= 1 = jumlah semua nilai dari level 1 hingga level tersebut.
    ///
    /// Contoh valuePerLevel = [0.5, 0.3, 0.2]:
    ///   Level 1 → 0.5
    ///   Level 2 → 0.5 + 0.3 = 0.8
    ///   Level 3 → 0.5 + 0.3 + 0.2 = 1.0
    /// </summary>
    public float GetCumulativeValue(int level)
    {
        if (level <= 0 || valuePerLevel == null) return 0f;

        float total = 0f;
        int maxIndex = Mathf.Min(level, valuePerLevel.Length);

        for (int i = 0; i < maxIndex; i++)
            total += valuePerLevel[i];

        return total;
    }

    /// <summary>
    /// True jika level yang diberikan adalah level maksimum atau melebihinya.
    /// </summary>
    public bool IsMaxLevel(int currentLevel) => currentLevel >= maxLevel;

    // ─────────────────────────────────────────────────────────────────────────
    // Validation
    // ─────────────────────────────────────────────────────────────────────────

    private void OnValidate()
    {
        if (pricePerLevel != null && pricePerLevel.Length != maxLevel)
            Debug.LogWarning($"[UpgradeDataSO] '{name}': pricePerLevel length ({pricePerLevel.Length}) harus sama dengan maxLevel ({maxLevel}).", this);

        if (valuePerLevel != null && valuePerLevel.Length != maxLevel)
            Debug.LogWarning($"[UpgradeDataSO] '{name}': valuePerLevel length ({valuePerLevel.Length}) harus sama dengan maxLevel ({maxLevel}).", this);
    }
}
