using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mengelola spawn minion selama boss fight.
/// Boss (EvilInaraBoss/EvilInaraPhaseController) hanya memanggil SpawnMinion(type, count).
/// Minion yang sudah di-spawn menggunakan AI existing (EnemyBehaviourController) sepenuhnya.
/// Spawner tidak mengatur AI minion sama sekali.
///
/// Prinsip:
///   EvilInara -> EvilInaraMinionSpawner -> Instantiate/Activate Minion
///                                       -> Existing Enemy AI berjalan sendiri
/// </summary>
public class EvilInaraMinionSpawner : MonoBehaviour
{
    [Header("Minion Prefabs")]
    [SerializeField] private GameObject redPrefab;
    [SerializeField] private GameObject bluePrefab;
    [SerializeField] private GameObject cyanPrefab;
    [SerializeField] private GameObject purplePrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnCooldown = 1f;
    [SerializeField] private int maxActiveMinions = 8;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private List<GameObject> activeMinions = new List<GameObject>();
    private float lastSpawnTime;
    private bool isStopped;

    // =========================================================
    // PUBLIC API
    // =========================================================

    /// <summary>
    /// Spawn minion sesuai archetype sejumlah count.
    /// Minion tidak akan di-spawn jika sudah mencapai maxActiveMinions.
    /// </summary>
    public void SpawnMinion(EnemyArchetype type, int count = 1)
    {
        if (isStopped) return;

        GameObject prefab = GetPrefab(type);
        if (prefab == null)
        {
            Debug.LogWarning($"[EvilInaraMinionSpawner] Prefab untuk {type} belum di-assign.", this);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            CleanupDestroyedMinions();

            if (activeMinions.Count >= maxActiveMinions)
            {
                Log($"Max minion reached ({maxActiveMinions}) — skip spawn");
                break;
            }

            Transform spawnPoint = GetNextSpawnPoint();
            if (spawnPoint == null) break;

            GameObject minion = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            activeMinions.Add(minion);

            Log($"Spawned {type} at {spawnPoint.name} | active={activeMinions.Count}");
        }
    }

    /// <summary>
    /// Hentikan semua spawn dan nonaktifkan minion yang aktif.
    /// Dipanggil saat boss mati.
    /// </summary>
    public void StopAllSpawns()
    {
        isStopped = true;
        Log("Spawn dihentikan");

        // Tidak langsung Destroy minion — biarkan mereka mati secara alami
        // atau dapat di-Destroy jika dibutuhkan untuk death sequence
    }

    /// <summary>
    /// Reset spawner (misalnya saat boss restart/debug).
    /// </summary>
    public void ResetSpawner()
    {
        isStopped = false;
        activeMinions.Clear();
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private GameObject GetPrefab(EnemyArchetype type)
    {
        switch (type)
        {
            case EnemyArchetype.Red:    return redPrefab;
            case EnemyArchetype.Blue:   return bluePrefab;
            case EnemyArchetype.Cyan:   return cyanPrefab;
            case EnemyArchetype.Purple: return purplePrefab;
            default:                    return null;
        }
    }

    private Transform GetNextSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[EvilInaraMinionSpawner] Tidak ada spawn point.", this);
            return null;
        }

        // Pilih spawn point secara acak
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    private void CleanupDestroyedMinions()
    {
        activeMinions.RemoveAll(m => m == null);
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[EvilInaraMinionSpawner] {message}", this);
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.yellow;
        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;
            Gizmos.DrawWireSphere(point.position, 0.5f);
        }
    }
}
