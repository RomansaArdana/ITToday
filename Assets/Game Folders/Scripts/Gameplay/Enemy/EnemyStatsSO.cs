using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyStats_Default",
    menuName = "The Day After/Enemy/Enemy Stats"
)]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float detectionAngle = 90f;
    [SerializeField] private float detectionSpeed = 1f;
    [SerializeField] private float detectionDecaySpeed = 1f;
    [SerializeField] private float detectionThreshold = 1f;

    [Header("Detection State")]
    [SerializeField] private float suspiciousThreshold = 0.35f;

    public float MoveSpeed => moveSpeed;

    public float DetectionRange => detectionRange;
    public float DetectionAngle => detectionAngle;
    public float DetectionSpeed => detectionSpeed;
    public float DetectionDecaySpeed => detectionDecaySpeed;
    public float DetectionThreshold => detectionThreshold;

    public float SuspiciousThreshold => suspiciousThreshold;
}