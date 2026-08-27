using UnityEngine;

public class EnemyCrouch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStateController stateController;
    [SerializeField] private Rigidbody2D rb;

    [Header("Crouch")]
    [SerializeField] private float crouchSpeedMultiplier = 0.75f;

    public bool IsCrouching { get; private set; }

    public float CrouchSpeedMultiplier =>
        crouchSpeedMultiplier;

    private void Awake()
    {
        stateController ??=
            GetComponent<EnemyStateController>();

        rb ??=
            GetComponent<Rigidbody2D>();
    }

    public void StartCrouch()
    {
        if (IsCrouching)
        {
            return;
        }

        if (stateController == null ||
            !stateController.IsFlee)
        {
            return;
        }

        IsCrouching = true;
    }

    public void StopCrouch()
    {
        if (!IsCrouching)
        {
            return;
        }

        IsCrouching = false;
    }
}