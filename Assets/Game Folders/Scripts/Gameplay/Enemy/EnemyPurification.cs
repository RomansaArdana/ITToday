using System;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyPurification : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyHealth health;

    [Header("Purification")]
    [SerializeField] private bool disableCollidersAfterPurification = false;

    private Collider2D[] colliders;

    public bool IsPurified { get; private set; }

    public event Action OnPurified;

    private void Awake()
    {
        health ??= GetComponent<EnemyHealth>();
        colliders = GetComponentsInChildren<Collider2D>();
    }

    private void OnEnable()
    {
        if (health != null) health.OnDeath += HandleHealthDepleted;
    }

    private void OnDisable()
    {
        if (health != null) health.OnDeath -= HandleHealthDepleted;
    }

    private void HandleHealthDepleted()
    {
        Purify();
    }

    public void Purify()
    {
        if (IsPurified) return;

        IsPurified = true;

        if (disableCollidersAfterPurification) DisableColliders();

        OnPurified?.Invoke();

        Debug.Log($"[EnemyPurification] {gameObject.name} purified.", this);
    }

    private void DisableColliders()
    {
        if (colliders == null) return;

        foreach (Collider2D col in colliders)
        {
            if (col != null) col.enabled = false;
        }
    }
}