/// <summary>
/// Enum identifier untuk setiap jenis upgrade yang tersedia di Kedai.
/// Gunakan enum ini — jangan hardcode string di ShopUI atau UpgradeManager.
///
/// Menambah upgrade baru = tambah entry di sini + buat UpgradeDataSO asset baru.
/// </summary>
public enum UpgradeType
{
    LanternAOE,     // Jangkauan area serangan lentera — PRIORITAS sebelum Chapter 2
    MaxSanity,      // Kapasitas maksimum Sanity bar
    CloakDuration   // Durasi aktif Cloak of Invisibility — penting sebelum Chapter 3
}
