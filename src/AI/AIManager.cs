public class AIManager
{
    public AIAction GetAction(GameState state, User player)
    {
        var a = new MinimaxNode(state, player);

        return a.GetMove();
    }
}