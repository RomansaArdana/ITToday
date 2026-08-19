using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO stats;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStateController stateController;
    [SerializeField] private Transform visual;

    private Vector3 visualStartPosition;

    private bool isJumping;
    private float jumpTimer;
    private float cooldownTimer;

    public bool IsJumping => isJumping;

    private void Awake()
    {
        if (inputReader == null)
        {
            inputReader = GetComponent<PlayerInputReader>();
        }

        if (stateController == null)
        {
            stateController = GetComponent<PlayerStateController>();
        }

        if (visual != null)
        {
            visualStartPosition = visual.localPosition;
        }
    }

    private void Update()
    {
        if (stats == null || inputReader == null || visual == null)
        {
            return;
        }

        UpdateCooldown();

        if (!isJumping)
        {
            TryStartJump();
            return;
        }

        UpdateJump();
    }

    private void TryStartJump()
    {
        if (!inputReader.JumpPressed)
        {
            return;
        }

        if (cooldownTimer > 0f)
        {
            return;
        }

        if (stateController != null && !stateController.CanMove())
        {
            return;
        }

        StartJump();
    }

    private void StartJump()
    {
        isJumping = true;
        jumpTimer = 0f;
    }

    private void UpdateJump()
    {
        jumpTimer += Time.deltaTime;

        float normalizedTime = jumpTimer / stats.JumpDuration;
        normalizedTime = Mathf.Clamp01(normalizedTime);

        float height = Mathf.Sin(normalizedTime * Mathf.PI)
                       * stats.JumpHeight;

        Vector3 position = visualStartPosition;
        position.y += height;

        visual.localPosition = position;

        if (normalizedTime >= 1f)
        {
            FinishJump();
        }
    }

    private void FinishJump()
    {
        isJumping = false;
        jumpTimer = 0f;
        cooldownTimer = stats.JumpCooldown;

        visual.localPosition = visualStartPosition;
    }

    private void UpdateCooldown()
    {
        if (cooldownTimer <= 0f)
        {
            return;
        }

        cooldownTimer -= Time.deltaTime;
    }
}