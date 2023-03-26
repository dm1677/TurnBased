using System.Collections.Generic;
using static GameSystem;

public class ResourceHandler : IHandler
{
    const int _resourcesPerTurn = 1;

    readonly HashSet<GResource> resourceList = new HashSet<GResource>();

    public bool Validate(Action action)
    {
        return IsValidCreateAction(action);
    }

    public bool Process(Action action)
    {
        UpdateResourceList();
        AddTurnResources();
        return true;
    }

    public void Reverse()
    {
        ReverseTurnResources();
    }

    bool IsValidCreateAction(Action action)
    {
        if (action is CreateAction createAction)
        {
            if (!GameSystem.Map.IsPassable(createAction.X, createAction.Y)) return false;

            foreach (GResource resource in resourceList)
            {
                var activePlayerOwnsResource = GameSystem.Game.Turn.CheckEntityOwnedByActivePlayer(resource.Parent);
                if (activePlayerOwnsResource)
                {
                    var unitType = (Unit)createAction.UnitType;
                    var cost = CostToBuildUnit(unitType, resource);

                    if (cost == -1) return false;
                    return (cost <= resource.Value);
                }
            }
        }
        return true;
    }
    
    void AddTurnResources()
    {
        foreach (GResource resource in resourceList)
            if (!GameSystem.Game.Turn.CheckEntityOwnedByActivePlayer(resource.Parent))
                resource.Value += _resourcesPerTurn;
    }

    void ReverseTurnResources()
    {
        foreach (GResource resource in resourceList)
            if (GameSystem.Game.Turn.CheckEntityOwnedByActivePlayer(resource.Parent))
                resource.Value -= _resourcesPerTurn;
    }

    void UpdateResourceList()
    {
        var list = GameSystem.EntityManager.GetEntityList().Keys;
        foreach (Entity entity in list)
        {
            var resource = entity.GetComponent<GResource>();
            if (resource != null) resourceList.Add(resource);
        }
    }

    //Returns -1 if not enough resources
    public static int CostToBuildUnit(Unit unitType, GResource resource)
    {
        var cost = ComponentFactory.Instance().UnitCost(unitType);
        if (resource.Value >= cost)
            return cost;
        else
            return -1;
    }

}