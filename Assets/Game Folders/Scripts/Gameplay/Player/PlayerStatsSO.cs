using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerStats_Inara",
    menuName = "The Day After/Player/Player Stats"
)]
public class PlayerStatsSO : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 25f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 0.75f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float jumpCooldown = 0.1f;

    [Header("Stealth")]
    [SerializeField] private float hideTransitionDuration = 0.2f;
    [SerializeField] private float hideCooldown = 0.25f;

    [Header("Sanity")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float sanityRecoveryRate = 25f;
    [SerializeField] private float sanityDrainRate = 5f;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 1.5f;

    public float MoveSpeed => moveSpeed;
    public float Acceleration => acceleration;
    public float Deceleration => deceleration;

    public float JumpHeight => jumpHeight;
    public float JumpDuration => jumpDuration;
    public float JumpCooldown => jumpCooldown;

    public float HideTransitionDuration => hideTransitionDuration;
    public float HideCooldown => hideCooldown;

    public float MaxSanity => maxSanity;
    public float SanityRecoveryRate => sanityRecoveryRate;
    public float SanityDrainRate => sanityDrainRate;

    public float InteractionRange => interactionRange;
}