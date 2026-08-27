using UnityEngine;

public class PlayerFacingController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visual;
    [SerializeField] private PlayerInputReader inputReader;

    [Header("Facing")]
    [SerializeField] private bool facingRight = true;

    public bool IsFacingRight => facingRight;
    public Vector2 FacingDirection =>
        facingRight ? Vector2.right : Vector2.left;

    private void Awake()
    {
        if (visual == null)
        {
            Transform visualChild =
                transform.Find("Visual");

            if (visualChild != null)
            {
                visual = visualChild;
            }
        }

        if (inputReader == null)
        {
            inputReader =
                GetComponent<PlayerInputReader>();
        }

        ApplyFacing();
    }

    private void Update()
    {
        if (inputReader == null)
        {
            return;
        }

        UpdateFacingDirection();
    }

    private void UpdateFacingDirection()
    {
        float moveX = inputReader.MoveInput.x;

        if (moveX > 0.01f)
        {
            SetFacingRight(true);
        }
        else if (moveX < -0.01f)
        {
            SetFacingRight(false);
        }
    }

    private void SetFacingRight(bool value)
    {
        if (facingRight == value)
        {
            return;
        }

        facingRight = value;

        ApplyFacing();
    }

    private void ApplyFacing()
    {
        if (visual == null)
        {
            return;
        }

        Vector3 scale = visual.localScale;

        scale.x =
            Mathf.Abs(scale.x) *
            (facingRight ? 1f : -1f);

        visual.localScale = scale;
    }
}