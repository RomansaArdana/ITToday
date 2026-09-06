using UnityEngine;

/// <summary>
/// Komponen yang ditempel secara dinamis ke setiap minion di boss arena
/// oleh BossPhaseManager. Melacak apakah minion sudah dipurnikan/mati
/// tanpa memodifikasi script musuh yang sudah ada.
/// </summary>
public class BossMinionTracker : MonoBehaviour
{
    private BossPhaseManager phaseManager;
    private bool hasNotified = false;

    // Nama state mati dari EnemyStateController berdasarkan pola kode yang ada
    private EnemyStateController stateController;

    public void Initialize(BossPhaseManager manager)
    {
        phaseManager = manager;
        stateController = GetComponent<EnemyStateController>();

        if (stateController == null)
            // Coba cari di parent/children
            stateController = GetComponentInChildren<EnemyStateController>();
    }

    private void Update()
    {
        if (hasNotified) return;
        if (stateController == null) return;

        // Cek apakah musuh sudah mati (IsDead ada di EnemyStateController)
        if (stateController.IsDead)
            NotifyPurified();
    }

    private void OnDestroy()
    {
        // Safety net: jika objek dihancurkan tanpa melewati Update
        if (!hasNotified)
            NotifyPurified();
    }

    private void NotifyPurified()
    {
        if (hasNotified) return;
        hasNotified = true;
        phaseManager?.OnMinionPurified(gameObject);
    }
}
