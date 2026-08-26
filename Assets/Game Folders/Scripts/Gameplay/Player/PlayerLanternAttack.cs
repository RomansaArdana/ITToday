using UnityEngine;

public class PlayerLanternAttack : MonoBehaviour
{
    [Header("Lantern")]
    [SerializeField] private GameObject lanternObject;

    private IPlayerInput input;
    private float originalLocalX;
    private bool lastFacingRight = true;

    private void Awake()
    {
        input = GetComponent<IPlayerInput>();

        if (input == null)
            Debug.LogError("[PlayerLanternAttack] IPlayerInput tidak ditemukan!");

        if (lanternObject != null)
        {
            originalLocalX = lanternObject.transform.localPosition.x;
            lanternObject.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateFacingDirection();

        if (lanternObject != null)
            lanternObject.SetActive(input.AttackHeld);
    }

    private void UpdateFacingDirection()
    {
        float moveX = input.MoveInput.x;

        if (moveX > 0.01f)
            lastFacingRight = true;
        else if (moveX < -0.01f)
            lastFacingRight = false;

        if (lanternObject != null)
        {
            Vector3 localPos = lanternObject.transform.localPosition;
            localPos.x = lastFacingRight ? Mathf.Abs(originalLocalX) : -Mathf.Abs(originalLocalX);
            lanternObject.transform.localPosition = localPos;
        }
    }
}
