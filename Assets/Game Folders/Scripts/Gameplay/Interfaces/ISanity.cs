public interface ISanity
{
    float CurrentSanity { get; }
    float MaxSanity { get; }
    bool IsDepleted { get; }

    void Drain(float amount);
    void Restore(float amount);
    void RestoreFull();
}