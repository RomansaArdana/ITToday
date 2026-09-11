using System;
using UnityEngine;

/// <summary>
/// Mengelola phase progression boss berdasarkan HP saat ini.
/// Dipanggil oleh EvilInaraBoss setiap kali HP boss berkurang.
///
/// Progression:
///   HP 6 -> Phase 1: Boss only
///   HP 5 -> Phase 2: Boss + Red
///   HP 4 -> Phase 3: Boss + Red + Blue
///   HP 3 -> Phase 4: Boss + Red + Blue + Cyan
///   HP 2 -> Phase 5: Boss + Red + Blue + Cyan + Purple
///   HP 1 -> Final Phase: All types
/// </summary>
public class EvilInaraPhaseController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EvilInaraMinionSpawner minionSpawner;

    [Header("Phase Spawn Count")]
    [SerializeField] private int phase2RedCount = 1;
    [SerializeField] private int phase3BlueCount = 1;
    [SerializeField] private int phase4CyanCount = 1;
    [SerializeField] private int phase5PurpleCount = 1;

    [Header("Final Phase (HP 1)")]
    [SerializeField] private int finalRedCount = 2;
    [SerializeField] private int finalBlueCount = 1;
    [SerializeField] private int finalCyanCount = 1;
    [SerializeField] private int finalPurpleCount = 1;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    public int CurrentPhase { get; private set; } = 1;

    /// <summary>Dipanggil setiap kali phase berubah.</summary>
    public event Action<int> OnPhaseChanged;

    private void Awake()
    {
        if (minionSpawner == null)
            minionSpawner = GetComponent<EvilInaraMinionSpawner>();
    }

    /// <summary>
    /// Evaluasi phase berdasarkan HP saat ini.
    /// Dipanggil oleh EvilInaraBoss saat HandleHPChanged.
    /// </summary>
    public void EvaluatePhase(int currentHP)
    {
        int newPhase = HPToPhase(currentHP);

        if (newPhase == CurrentPhase) return;

        CurrentPhase = newPhase;
        Log($"Phase {CurrentPhase} dimulai (HP={currentHP})");

        OnPhaseChanged?.Invoke(CurrentPhase);
        ExecutePhase(CurrentPhase);
    }

    private int HPToPhase(int hp)
    {
        switch (hp)
        {
            case 6: return 1;
            case 5: return 2;
            case 4: return 3;
            case 3: return 4;
            case 2: return 5;
            case 1: return 6; // Final Phase
            default: return 1;
        }
    }

    private void ExecutePhase(int phase)
    {
        if (minionSpawner == null)
        {
            Debug.LogWarning("[EvilInaraPhaseController] MinionSpawner tidak ditemukan.", this);
            return;
        }

        switch (phase)
        {
            case 2:
                Log("Spawn Red");
                minionSpawner.SpawnMinion(EnemyArchetype.Red, phase2RedCount);
                break;

            case 3:
                Log("Spawn Blue");
                minionSpawner.SpawnMinion(EnemyArchetype.Blue, phase3BlueCount);
                break;

            case 4:
                Log("Spawn Cyan");
                minionSpawner.SpawnMinion(EnemyArchetype.Cyan, phase4CyanCount);
                break;

            case 5:
                Log("Spawn Purple");
                minionSpawner.SpawnMinion(EnemyArchetype.Purple, phase5PurpleCount);
                break;

            case 6:
                Log("FINAL PHASE — Spawn All");
                minionSpawner.SpawnMinion(EnemyArchetype.Red, finalRedCount);
                minionSpawner.SpawnMinion(EnemyArchetype.Blue, finalBlueCount);
                minionSpawner.SpawnMinion(EnemyArchetype.Cyan, finalCyanCount);
                minionSpawner.SpawnMinion(EnemyArchetype.Purple, finalPurpleCount);
                break;
        }
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[EvilInaraPhaseController] {message}", this);
    }
}
