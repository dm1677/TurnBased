public class AICreateAction : AIAction
{
    public int x, y;
    public Unit unitType;
    public User owner;

    public AICreateAction(int x, int y, Unit unitType, User owner)
    {
        this.x = x;
        this.y = y;
        this.unitType = unitType;
        this.owner = owner;
    }
}