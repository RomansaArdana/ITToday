using UnityEngine;

/// <summary>
/// Mengontrol lantern player.
/// Lantern muncul selama tombol Attack (F) ditekan, hilang saat dilepas.
///
/// Cara kerja:
///   1. Script ini membaca AttackHeld dari IPlayerInput setiap frame.
///   2. Jika AttackHeld == true  -> lanternObject diaktifkan (SetActive true).
///   3. Jika AttackHeld == false -> lanternObject dimatikan (SetActive false).
///
/// Setup di Unity Inspector:
///   - Tambah component ini ke Player GameObject.
///   - Assign "Lantern Object" ke child GameObject lantern yang sudah ada di prefab.
/// </summary>
public class PlayerLanternAttack : MonoBehaviour
{
    [Header("Lantern")]
    [Tooltip("Child GameObject lantern yang akan di-toggle. Drag & drop dari hierarchy player.")]
    [SerializeField] private GameObject lanternObject;

    // Referensi ke sistem input player (diambil otomatis dari GameObject yang sama)
    private IPlayerInput input;

    // Simpan posisi X awal lantern (dari Inspector/prefab)
    private float originalLocalX;

    // Arah terakhir player menghadap (true = kanan)
    private bool lastFacingRight = true;

    private void Awake()
    {
        input = GetComponent<IPlayerInput>();

        if (input == null)
            Debug.LogError("[PlayerLanternAttack] IPlayerInput tidak ditemukan! " +
                           "Pastikan PlayerInputReader ada di GameObject yang sama.");

        if (lanternObject == null)
            Debug.LogWarning("[PlayerLanternAttack] Lantern Object belum di-assign di Inspector!");

        // Catat posisi X awal lantern dari prefab/scene
        if (lanternObject != null)
        {
            originalLocalX = lanternObject.transform.localPosition.x;
            lanternObject.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateFacingDirection();

        // Ketika F ditekan  : AttackHeld = true  -> lantern nyala
        // Ketika F dilepas  : AttackHeld = false -> lantern mati
        if (lanternObject != null)
            lanternObject.SetActive(input.AttackHeld);
    }

    private void UpdateFacingDirection()
    {
        float moveX = input.MoveInput.x;

        // Hanya update arah jika player bergerak horizontal
        if (moveX > 0.01f)
            lastFacingRight = true;
        else if (moveX < -0.01f)
            lastFacingRight = false;

        // Flip posisi X lantern sesuai arah yang terakhir dihadapi player.
        // Jika kanan : pakai originalLocalX (posisi asli)
        // Jika kiri  : balik ke -originalLocalX (mirror)
        if (lanternObject != null)
        {
            Vector3 localPos = lanternObject.transform.localPosition;
            localPos.x = lastFacingRight ? Mathf.Abs(originalLocalX) : -Mathf.Abs(originalLocalX);
            lanternObject.transform.localPosition = localPos;
        }
    }
}
