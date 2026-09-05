using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerCrouch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStateController stateController;
    [SerializeField] private PlayerAnimator playerAnimator;

    [Header("Collider — Standing")]
    [SerializeField] private float standingHeight = 3.5f;
    [SerializeField] private float standingOffsetY = 1.77f;

    [Header("Collider — Crouching")]
    [SerializeField] private float crouchHeight = 2.6f;
    [SerializeField] private float crouchOffsetY = 1.3f;

    [Header("Ceiling Check")]
    [SerializeField] private LayerMask ceilingLayer = ~0;
    [SerializeField] private float ceilingCheckMargin = 0.05f;
    [SerializeField, Range(0.5f, 1f)] private float ceilingCheckWidth = 0.8f;

    [Header("Debug")]
    [SerializeField] private bool showGizmo = true;

    private CapsuleCollider2D capsule;

    public bool IsCrouching { get; private set; }

    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider2D>();

        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (stateController == null) stateController = GetComponent<PlayerStateController>();
        if (playerAnimator == null) playerAnimator = GetComponent<PlayerAnimator>();

        ApplyStandingCollider();
    }

    private void Update()
    {
        if (inputReader == null || stateController == null) return;

        if (inputReader.CrouchHeld)
            TryEnterCrouch();
        else
            TryExitCrouch();
    }

    private void TryEnterCrouch()
    {
        if (IsCrouching) return;
        if (!stateController.CanCrouch()) return;

        ApplyCrouchCollider();
        IsCrouching = true;
        stateController.EnterCrouch();

        if (playerAnimator != null)
            playerAnimator.SetCrouching(true);
    }

    private void TryExitCrouch()
    {
        if (!IsCrouching) return;
        if (!CanStandUp()) return;

        ApplyStandingCollider();
        IsCrouching = false;
        stateController.ExitCrouch();

        if (playerAnimator != null)
            playerAnimator.SetCrouching(false);
    }

    private bool CanStandUp()
    {
        if (capsule == null) return true;

        float currentTop = capsule.bounds.max.y;
        float neededClearance = standingHeight - crouchHeight + ceilingCheckMargin;

        if (neededClearance <= 0f)
            return true;

        Vector2 checkOrigin = new Vector2(capsule.bounds.center.x, currentTop);
        Vector2 checkSize = new Vector2(capsule.bounds.size.x * ceilingCheckWidth, 0.05f);

        LayerMask mask = ceilingLayer.value == 0 ? ~0 : ceilingLayer;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(checkOrigin, checkSize, 0f, Vector2.up, neededClearance, mask);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.isTrigger) continue;
            if (hit.collider.transform.root == transform.root) continue;

            return false;
        }

        return true;
    }

    private void ApplyCrouchCollider()
    {
        if (capsule == null) return;

        capsule.size = new Vector2(capsule.size.x, crouchHeight);
        capsule.offset = new Vector2(capsule.offset.x, crouchOffsetY);
    }

    private void ApplyStandingCollider()
    {
        if (capsule == null) return;

        capsule.size = new Vector2(capsule.size.x, standingHeight);
        capsule.offset = new Vector2(capsule.offset.x, standingOffsetY);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        Vector3 pivot = transform.position;

        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);

        Vector3 standCenter = pivot + new Vector3(0f, standingOffsetY, 0f);
        Gizmos.DrawWireCube(
            standCenter,
            new Vector3(0.5f, standingHeight, 0f)
        );

        if (Application.isPlaying && IsCrouching && capsule != null)
        {
            float neededClearance = standingHeight - crouchHeight + ceilingCheckMargin;
            float currentTop = capsule.bounds.max.y;

            Gizmos.color = CanStandUp() ? Color.green : Color.red;

            Vector3 ceilingCheckCenter = new Vector3(
                capsule.bounds.center.x,
                currentTop + neededClearance * 0.5f,
                0f
            );

            Gizmos.DrawWireCube(
                ceilingCheckCenter,
                new Vector3(
                    capsule.bounds.size.x * ceilingCheckWidth,
                    neededClearance,
                    0f
                )
            );
        }
    }
}