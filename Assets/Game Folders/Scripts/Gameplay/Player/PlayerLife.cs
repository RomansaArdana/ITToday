using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    [SerializeField] private int startingAttempts = 3;

    public int Attempts { get; private set; }

    private void Awake()
    {
        Attempts = startingAttempts;
    }

    public bool ConsumeAttempt()
    {
        if (Attempts <= 0) return false;

        Attempts--;

        return Attempts > 0;
    }

    public void ResetAttempts()
    {
        Attempts = startingAttempts;
    }
}