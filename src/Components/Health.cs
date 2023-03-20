public class Health : Component
{
    public int MaxHP { get; private set; } = 0;
    public int CurrentHP { get; set; } = 0;
    public int Regeneration { get; private set; } = 0;

    public int TurnsSinceRegeneration { get; set; } = 0; //for reversing replays - maybe could use this in future for regenerating every x turns

    public Health(int maxHP, int regeneration)
    {
        MaxHP = maxHP;
        CurrentHP = maxHP;
        Regeneration = regeneration;
        TurnsSinceRegeneration = 0;
    }
}