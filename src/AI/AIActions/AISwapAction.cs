public class AISwapAction : AIAction
{
    public UnitState swappedUnit;
    public UnitState swappingUnit;

    public AISwapAction(UnitState swapped, UnitState swapper)
    {
        swappedUnit = swapped;
        swappingUnit = swapper;
    }
}