using System;
using System.Collections.Generic;

public class GameState
{
    public int mapWidth { get; protected set; }
    public int mapHeight { get; protected set; }

    private bool[,] map;

    public readonly List<UnitState> units = new List<UnitState>();
    public User playerToMove { get; protected set; }

    public GameState(int mapWidth, int mapHeight, User playerToMove)
    {
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.playerToMove = playerToMove;

        InitialiseMap();
    }

    private void InitialiseMap()
    {
        map = new bool[mapWidth, mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                map[x, y] = true;
            }
        }
    }

    public void AddUnit(UnitState unit)
    {
        units.Add(unit);
        if(unit.X >= 0 && unit.X <= map.GetUpperBound(0) && unit.Y >= 0 && unit.Y <= map.GetUpperBound(1))
            map[unit.X, unit.Y] = false;
    }

    public bool Passable(int x, int y)
    {
        if (x < 0 || x > map.GetUpperBound(0) || y < 0 || y > map.GetUpperBound(1))
            throw new ArgumentOutOfRangeException("Trying to check passability of invalid location");
        return map[x, y];
    }

    public List<UnitState> GetEnemyUnits(User player)
    {
        var unitList = new List<UnitState>();
        foreach(UnitState unit in units)
        {
            if (unit.Owner != player && unit.Owner != User.Neutral)
                unitList.Add(unit);                
        }

        return unitList;
    }

    public bool IsGameOver()
    {
        bool playerKing = false;
        bool enemyKing = false;
        
        foreach (UnitState unit in units)
        {
            if (unit.UnitType == Unit.King && unit.Owner == playerToMove)
                playerKing = true;
            if (unit.UnitType == Unit.King && unit.Owner != playerToMove)
                enemyKing = true;
        }
        
        return !(playerKing && enemyKing);
    }

    //1 for win, -1 for loss, 0 for unknown
    public User GameResult()
    {
        List<UnitState> kings = new List<UnitState>(4);

        foreach(UnitState unit in units)
        {
            if (unit.UnitType == Unit.King)
                kings.Add(unit);
        }

        if (kings.Count >= 3) return User.Neutral;
        if (kings.Count == 2)
            if (kings[0].Owner == kings[1].Owner) return kings[0].Owner;
        if (kings.Count == 1) return kings[0].Owner;

        return User.Neutral;
    }

    public GameState Move(AIAction action)
    {
        GameState newState = new GameState(mapWidth, mapHeight, Owner.SwapPlayers(playerToMove));
        User currentPlayer = User.Neutral;
        int unitBuiltCost = 0;

        if (action is AIMoveAction moveAction)
        {
            currentPlayer = moveAction.unit.Owner;
            foreach(UnitState unit in units)
            {
                if (unit == moveAction.unit)
                //if (unit.GetHashCode() == moveAction.unit.GetHashCode())
                    newState.AddUnit(unit.Clone(moveAction.x, moveAction.y));
                else newState.AddUnit(unit.Clone());
            }
        }
        if (action is AIAttackAction attackAction)
        {
            currentPlayer = attackAction.attacker.Owner;
            foreach (UnitState unit in units)
            {
                bool unitIsKilled = AIAttackAction.UnitIsKilled(attackAction.attacker, attackAction.defender);

                if (unit == attackAction.attacker)
                //if (unit.GetHashCode() == attackAction.attacker.GetHashCode())
                {
                    if (unitIsKilled && unit.UnitClass.IsMoveAttacker)
                        newState.AddUnit(unit.Clone(attackAction.defender.X, attackAction.defender.Y));
                    else
                        newState.AddUnit(unit.Clone());
                }
                else if (unit == attackAction.defender)
                //else if (unit.GetHashCode() == attackAction.defender.GetHashCode())
                {
                    if (!unitIsKilled)
                        newState.AddUnit(unit.Clone(unit.Health - attackAction.attacker.UnitClass.Damage));
                }
                else
                    newState.AddUnit(unit.Clone());
            }
        }
        if (action is AICreateAction createAction)
        {
            currentPlayer = createAction.owner;
            newState.AddUnit(new UnitState(createAction.unitType, createAction.owner, 1, createAction.x, createAction.y));
            foreach (UnitState unit in units)
                newState.AddUnit(unit.Clone());
            unitBuiltCost = ComponentFactory.Instance().UnitCost(createAction.unitType);
        }
        if (action is AISwapAction swapAction)
        {
            List<UnitState> swaps = new List<UnitState>(2);
            foreach(UnitState unit in units)
            {
                if (unit == swapAction.swappedUnit)
                    newState.AddUnit(unit.Clone(swapAction.swappingUnit.X, swapAction.swappingUnit.Y));
                else if (unit == swapAction.swappingUnit)
                    newState.AddUnit(unit.Clone(swapAction.swappedUnit.X, swapAction.swappedUnit.Y));
                else
                    newState.AddUnit(unit.Clone());

            }
        }

        foreach(UnitState unit in units)
        {
            if (unit.UnitType == Unit.Resource)
            {
                if (unit.Owner == currentPlayer)
                    newState.AddUnit(unit.Clone(unit.Health + 1 - unitBuiltCost));//did i fuck up?
                else
                    newState.AddUnit(unit.Clone());
            }
        }

        return newState;
    }
}