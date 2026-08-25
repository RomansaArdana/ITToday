using UnityEngine;

public class GameplayStateInitializer : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.ChangeState(GameState.Gameplay);
    }
}