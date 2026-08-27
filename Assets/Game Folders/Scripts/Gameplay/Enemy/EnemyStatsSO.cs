using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyStats_Default",
    menuName = "The Day After/Enemy/Enemy Stats"
)]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private EnemyArchetype archetype =
        EnemyArchetype.Red;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float detectionAngle = 90f;
    [SerializeField] private float detectionSpeed = 1f;
    [SerializeField] private float detectionDecaySpeed = 1f;
    [SerializeField] private float detectionThreshold = 1f;

    [Header("Detection State")]
    [SerializeField, Range(0f, 1f)]
    private float suspiciousThreshold = 0.35f;

    [Header("Combat")]
    [SerializeField] private float maxHealth = 1f;

    [Header("Reward")]
    [SerializeField] private float purificationSanityReward = 10f;

    public EnemyArchetype Archetype =>
        archetype;

    public float MoveSpeed =>
        moveSpeed;

    public float DetectionRange =>
        detectionRange;

    public float DetectionAngle =>
        detectionAngle;

    public float DetectionSpeed =>
        detectionSpeed;

    public float DetectionDecaySpeed =>
        detectionDecaySpeed;

    public float DetectionThreshold =>
        detectionThreshold;

    public float SuspiciousThreshold =>
        suspiciousThreshold;

    public float MaxHealth =>
        maxHealth;

    public float PurificationSanityReward =>
        purificationSanityReward;
}