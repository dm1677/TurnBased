public class Owner : Component
{
    public User ownedBy = User.Neutral;

    public static User SwapPlayers(User player)
    {
        if (player == User.Player) return User.Enemy;
        else return User.Player;
    }
}