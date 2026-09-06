using System;
using UnityEngine;

/// <summary>
/// Interactable khusus untuk Album Foto di Ruang Kenangan.
/// Memancarkan event OnAlbumInteracted yang di-listen oleh BossArenaSetup
/// untuk memulai transisi ke Boss Battle.
/// </summary>
public class AlbumPhotoInteractable : InteractableBase
{
    [Header("Album Photo")]
    [SerializeField] private string albumName = "Album Foto";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    /// <summary>
    /// Event yang dikirim ke BossArenaSetup saat pemain berinteraksi.
    /// </summary>
    public event Action OnAlbumInteracted;

    private bool hasBeenInteracted = false;

    public override void Interact(GameObject interactor)
    {
        if (hasBeenInteracted)
        {
            Log($"{albumName} sudah pernah diinteraksi.");
            return;
        }

        hasBeenInteracted = true;
        SetInteractionEnabled(false); // Cegah trigger ganda

        Log($"{albumName} diinteraksi oleh {interactor.name} — memulai Boss Battle!");

        // Kembalikan kontrol ke player
        PlayerStateController stateController = interactor.GetComponent<PlayerStateController>();
        stateController?.ExitInteraction();

        // Trigger event ke BossArenaSetup
        OnAlbumInteracted?.Invoke();
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[AlbumPhoto] {message}", this);
    }
}
