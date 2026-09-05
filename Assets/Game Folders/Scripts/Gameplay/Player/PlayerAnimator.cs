using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int ParamSpeed = Animator.StringToHash("Speed");
    private static readonly int ParamIsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int ParamIsCrouching = Animator.StringToHash("isCrouching");
    private static readonly int ParamIsCloaked = Animator.StringToHash("isCloaked");
    private static readonly int ParamIsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int ParamTriggerAttack = Animator.StringToHash("TriggerAttack");
    private static readonly int ParamTriggerDrowned = Animator.StringToHash("TriggerDrowned");
    private static readonly int ParamSanityLost = Animator.StringToHash("TriggerSanityLost");

    [Header("References")]
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void SetSpeed(float speed)
    {
        if (animator == null) return;
        animator.SetFloat(ParamSpeed, speed);
    }

    public void SetGrounded(bool isGrounded)
    {
        if (animator == null) return;
        animator.SetBool(ParamIsGrounded, isGrounded);
    }

    public void SetCrouching(bool isCrouching)
    {
        if (animator == null) return;
        animator.SetBool(ParamIsCrouching, isCrouching);
    }

    public void SetCloaked(bool isCloaked)
    {
        if (animator == null) return;
        animator.SetBool(ParamIsCloaked, isCloaked);
    }

    public void SetAttacking(bool isAttacking)
    {
        if (animator == null) return;
        animator.SetBool(ParamIsAttacking, isAttacking);
    }

    public void TriggerAttack()
    {
        if (animator == null) return;
        animator.SetTrigger(ParamTriggerAttack);
    }

    public void TriggerDrowned()
    {
        if (animator == null) return;
        animator.SetTrigger(ParamTriggerDrowned);
    }

    public void TriggerSanityLost()
    {
        if (animator == null) return;
        animator.SetTrigger(ParamSanityLost);
    }
}