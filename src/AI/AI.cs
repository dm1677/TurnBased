using System;
using System.Collections.Generic;

public class AI
{
    Random random = new Random();

    public static List<AIAction> GetLegalActions(GameState state, User player)
    {
        var legalActions = new List<AIAction>();
        int money = -1;

        foreach (UnitState unit in state.units)
        {
            if (unit.Owner != player)
                continue;

            if (unit.UnitType == Unit.Resource)
            {
                money = unit.Health;
                continue;
            }

            for (int y = 0; y < state.mapHeight; y++)
            {
                for (int x = 0; x < state.mapWidth; x++)
                {
                    if (unit.UnitClass.Speed > 0 && state.Passable(x, y) && AIMoveAction.CheckMovement(unit.UnitClass.MovementType, state, unit, x, y))
                    {
                        legalActions.Add(new AIMoveAction(x, y, unit));
                    }
                }
            }

            if (unit.UnitClass.Damage > 0 && unit.UnitClass.Range > 0)
            {
                var validTargets = AIAttackAction.Attack(unit.UnitClass.AttackType, state, unit);

                foreach (UnitState enemyUnit in validTargets)
                    legalActions.Add(new AIAttackAction(unit, enemyUnit));
            }
        
            if (unit.UnitType == Unit.Prawn)
            {
                foreach (UnitState unit2 in state.units)
                {
                    if (unit.Owner != User.Player) continue;
                    if (unit == unit2) continue;
                    if (unit.UnitType == Unit.Resource) continue;

                    legalActions.Add(new AISwapAction(unit, unit2));
                }
            }

        }

        if (money == -1) throw new Exception("Undefined resource");

        if (money > 2)
        {
            var values = Enum.GetValues(typeof(Unit));
            foreach (Unit unitType in values)
            {
                if (unitType == Unit.King || unitType == Unit.Resource || unitType == Unit.Tree) continue;

                if (money >= ComponentFactory.Instance().UnitCost(unitType))
                    for (int y = 0; y < state.mapHeight; y++)
                    {
                        for (int x = 0; x < state.mapWidth; x++)
                        {
                            if (state.Passable(x, y))
                            {
                                legalActions.Add(new AICreateAction(x, y, unitType, player));
                            }
                        }
                    }
            }
        }//--------------------------------------------------------------------------------CREATEACTIONS -- COMMENT BACK IN!!!!!
        return legalActions;
    }

    public static List<AIAction> GetPrunedActions(GameState state, User player)
    {
        var legalActions = new List<AIAction>();
        int money = -1;

        List<Coords> enemyKingDiagonals = new List<Coords>(8);
        List<UnitState> enemyKings = new List<UnitState>(2);
        foreach (UnitState unit in state.units)
        {
            if (unit.Owner != player)
            {
                if (unit.UnitType == Unit.King)
                {
                    if ((unit.X + 1 < 15) && (unit.Y + 1 < 15))
                        enemyKingDiagonals.Add(new Coords(unit.X + 1, unit.Y + 1));
                    if ((unit.X - 1 >= 0) && (unit.Y - 1 >= 0))
                        enemyKingDiagonals.Add(new Coords(unit.X - 1, unit.Y - 1));
                    if ((unit.X + 1 < 15) && (unit.Y - 1 >= 0))
                        enemyKingDiagonals.Add(new Coords(unit.X + 1, unit.Y - 1));
                    if ((unit.X - 1 >= 0) && (unit.Y + 1 < 15))
                        enemyKingDiagonals.Add(new Coords(unit.X - 1, unit.Y + 1));

                    enemyKings.Add(unit);
                }
                continue;
            }

            if (unit.UnitType == Unit.Resource)
            {
                money = unit.Health;
                continue;
            }

            var validTargets = AIAttackAction.Attack(unit.UnitClass.AttackType, state, unit);
            if (validTargets.Count > 0)
            {
                foreach (UnitState enemyUnit in validTargets)
                    legalActions.Add(new AIAttackAction(unit, enemyUnit));
            }
            if (unit.UnitType == Unit.Gobbo)
            {
                foreach(UnitState enemy in state.units)
                {
                    if (enemy.Owner == player) continue;
                    if (enemy.UnitType == Unit.Resource || enemy.UnitType == Unit.Tree) continue;

                    var list = AIAttackAction.Attack(enemy.UnitClass.AttackType, state, enemy);
                    if (list.Contains(unit))
                    {
                        for(int j = -3; j < 6; j++)
                        {
                            int x = unit.X + j;
                            int y = unit.Y + j;
                            if (x >= 0 && x < 15 && y >= 0 && y < 15 && state.Passable(x, y) && AIMoveAction.CheckMovement(unit.UnitClass.MovementType, state, unit, x, y))
                                legalActions.Add(new AIMoveAction(x, y, unit));

                            x = unit.X - j;
                            y = unit.Y - j;
                            if (x >= 0 && x < 15 && y >= 0 && y < 15 && state.Passable(x, y) && AIMoveAction.CheckMovement(unit.UnitClass.MovementType, state, unit, x, y))
                                legalActions.Add(new AIMoveAction(x, y, unit));
                        }
                    }
                }
                foreach (Coords coords in enemyKingDiagonals)
                {
                    if (state.Passable(coords.X, coords.Y) && AIMoveAction.CheckMovement(unit.UnitClass.MovementType, state, unit, coords.X, coords.Y))
                        legalActions.Add(new AIMoveAction(coords.X, coords.Y, unit));
                }
            }
            else if (unit.UnitType == Unit.Prawn)
            {
                foreach (UnitState unit2 in state.units)
                {
                    if (unit.Owner != User.Player) continue;
                    if (unit == unit2) continue;
                    if (unit.UnitType == Unit.Resource) continue;

                    legalActions.Add(new AISwapAction(unit, unit2));
                }
            }
            else
            {
                for (int y = 0; y < state.mapHeight; y++)
                {
                    for (int x = 0; x < state.mapWidth; x++)
                    {
                        if (state.Passable(x, y) && AIMoveAction.CheckMovement(unit.UnitClass.MovementType, state, unit, x, y))
                            legalActions.Add(new AIMoveAction(x, y, unit));
                    }
                }
            }
        }

        if (money == -1) throw new Exception("Undefined resource");

        if (money > 2)
        {
            bool create = false;
            foreach (Coords coords in enemyKingDiagonals)
            {
                create = false;
                if (state.Passable(coords.X, coords.Y))
                {
                    UnitState tempUnit = new UnitState(Unit.Gobbo, player, 8, coords.X, coords.Y);
                    state.AddUnit(tempUnit);
                    
                    foreach(UnitState unit in state.units)
                    {
                        if (unit.UnitType == Unit.Resource || unit.UnitType == Unit.Tree) continue;
                        if (unit.Owner != player && unit.UnitClass.Damage >= tempUnit.UnitClass.MaxHP)
                        {
                            var list = AIAttackAction.Attack(unit.UnitClass.AttackType, state, unit);
                            if (list.Count > 0)
                            {
                                foreach (UnitState attackedUnit in list)
                                {
                                    //Godot.Logging.Log(enemyKingDiagonals.Count + " Diagonals checked :: Attacks: " + list.Count);
                                    if (attackedUnit != tempUnit)
                                        create = true;
                                }
                            }
                        }
                    }
                    state.units.Remove(tempUnit);
                    if (!create)
                        legalActions.Add(new AICreateAction(coords.X, coords.Y, Unit.Gobbo, player));

                    if (money >= ComponentFactory.Instance().UnitCost(Unit.Knight))
                        legalActions.Add(new AICreateAction(coords.X, coords.Y, Unit.Knight, player));
                }
            }

            if(money >= ComponentFactory.Instance().UnitCost(Unit.Building))
            {
                foreach(UnitState king in enemyKings)
                {
                    for(int i = king.X - 6; i <= king.X + 6; i++)
                    {
                        if (i >= 0 && i < 15 && state.Passable(i, king.Y))
                        {
                            UnitState tempUnit = new UnitState(Unit.Building, player, 8, i, king.Y);
                            state.AddUnit(tempUnit);

                            var list = AIAttackAction.Attack(tempUnit.UnitClass.AttackType, state, tempUnit);
                            if (list.Count > 1)
                                legalActions.Add(new AICreateAction(tempUnit.X, tempUnit.Y, tempUnit.UnitType, player));
                            state.units.Remove(tempUnit);
                        }
                    }
                    for (int i = king.Y - 6; i <= king.Y + 6; i++)
                    {
                        if (i >= 0 && i < 15 && state.Passable(king.X, i))
                        {
                            UnitState tempUnit = new UnitState(Unit.Building, player, 8, king.X, i);
                            state.AddUnit(tempUnit);

                            var list = AIAttackAction.Attack(tempUnit.UnitClass.AttackType, state, tempUnit);
                            if (list.Count > 1)
                                legalActions.Add(new AICreateAction(tempUnit.X, tempUnit.Y, tempUnit.UnitType, player));
                            state.units.Remove(tempUnit);
                        }
                    }

                    if(king.X == 0 && !state.Passable(king.X + 1, king.Y))
                    {
                        if (king.Y - 1 >= 0 && state.Passable(king.X, king.Y - 1))
                            legalActions.Add(new AICreateAction(king.X, king.Y - 1, Unit.Building, player));
                        if (king.Y + 1 < 15 && state.Passable(king.X, king.Y + 1))
                            legalActions.Add(new AICreateAction(king.X, king.Y + 1, Unit.Building, player));
                    }
                    if (king.X == 14 && !state.Passable(king.X - 1, king.Y))
                    {
                        if (king.Y - 1 >= 0 && state.Passable(king.X, king.Y - 1))
                            legalActions.Add(new AICreateAction(king.X, king.Y - 1, Unit.Building, player));
                        if (king.Y + 1 < 15 && state.Passable(king.X, king.Y + 1))
                            legalActions.Add(new AICreateAction(king.X, king.Y + 1, Unit.Building, player));
                    }
                    if (king.Y == 14 && !state.Passable(king.X, king.Y - 1))
                    {
                        if (king.X - 1 >= 0 && state.Passable(king.X - 1, king.Y))
                            legalActions.Add(new AICreateAction(king.X - 1, king.Y, Unit.Building, player));
                        if (king.X + 1 < 15 && state.Passable(king.X + 1, king.Y))
                            legalActions.Add(new AICreateAction(king.X + 1, king.Y, Unit.Building, player));
                    }
                    if (king.Y == 0 && !state.Passable(king.X, king.Y + 1))
                    {
                        if (king.X - 1 >= 0 && state.Passable(king.X - 1, king.Y))
                            legalActions.Add(new AICreateAction(king.X - 1, king.Y, Unit.Building, player));
                        if (king.X + 1 < 15 && state.Passable(king.X + 1, king.Y))
                            legalActions.Add(new AICreateAction(king.X + 1, king.Y, Unit.Building, player));
                    }
                }
            }
        }
        return legalActions;
    }

    public static List<AIAction> GetGobboActions(GameState state, User player)
    {
        List<AIAction> list = new List<AIAction>();
        List<UnitState> enemyKings = new List<UnitState>();
        List<Coords> enemyKingDiagonals = new List<Coords>();
        int money = 0;
        int attackActions = 0;

        foreach (UnitState unit in state.units)
        {
            if (unit.Owner != player)
            {
                if (unit.UnitType == Unit.King)
                {
                    if ((unit.X + 1 < 15) && (unit.Y + 1 < 15))
                        enemyKingDiagonals.Add(new Coords(unit.X + 1, unit.Y + 1));
                    if ((unit.X - 1 >= 0) && (unit.Y - 1 >= 0))
                        enemyKingDiagonals.Add(new Coords(unit.X - 1, unit.Y - 1));
                    if ((unit.X + 1 < 15) && (unit.Y - 1 >= 0))
                        enemyKingDiagonals.Add(new Coords(unit.X + 1, unit.Y - 1));
                    if ((unit.X - 1 >= 0) && (unit.Y + 1 < 15))
                        enemyKingDiagonals.Add(new Coords(unit.X - 1, unit.Y + 1));

                    enemyKings.Add(unit);
                }
                continue;
            }

            if (unit.UnitType == Unit.Resource)
            {
                money = unit.Health;
                continue;
            }

            var validTargets = AIAttackAction.Attack(unit.UnitClass.AttackType, state, unit);
            if (validTargets.Count > 0)
            {
                foreach (UnitState enemyUnit in validTargets)
                {
                    list.Add(new AIAttackAction(unit, enemyUnit));
                    attackActions++;
                }
            }
            else
            {
                if (unit.UnitType == Unit.Gobbo && money < 3)
                {
                    foreach (Coords coords in enemyKingDiagonals)
                    {
                        if (state.Passable(coords.X, coords.Y) && AIMoveAction.CheckMovement(unit.UnitClass.MovementType, state, unit, coords.X, coords.Y))
                            list.Add(new AIMoveAction(coords.X, coords.Y, unit));
                    }
                }
            }

            if (unit.UnitType == Unit.King)
            {
                for(int y = unit.Y; y <= unit.Y + 2; y++)
                {
                    if (y >= 0 && y < 15)
                    {
                        if (state.Passable(unit.X, y))
                            list.Add(new AIMoveAction(unit.X, y, unit));
                        else break;
                    }
                }
                for (int y = unit.Y; y >= unit.Y - 2; y--)
                {
                    if (y >= 0 && y < 15)
                    {
                        if (state.Passable(unit.X, y))
                            list.Add(new AIMoveAction(unit.X, y, unit));
                        else break;
                    }
                }
                for (int x = unit.X; x <= unit.X + 2; x++)
                {
                    if (x >= 0 && x < 15)
                    {
                        if (state.Passable(x, unit.Y))
                            list.Add(new AIMoveAction(x, unit.Y, unit));
                        else break;
                    }
                }
                for (int x = unit.X; x >= unit.X - 2; x--)
                {
                    if (x >= 0 && x < 15)
                    {
                        if (state.Passable(x, unit.Y))
                            list.Add(new AIMoveAction(x, unit.Y, unit));
                        else break;
                    }
                }
            }
            
        }

        if (money > 2 && attackActions == 0)
        {
            foreach (Coords coords in enemyKingDiagonals)
            {
                list.Add(new AICreateAction(coords.X, coords.Y, Unit.Gobbo, player));
                if (money > 11)
                    list.Add(new AICreateAction(coords.X, coords.Y, Unit.Knight, player));
            }
        }
                
        return list;
    }

    void PrintUnitInfo(UnitState unit)
    {
        Godot.Logging.Log("Unit Type: " + unit.UnitType);
        Godot.Logging.Log("Health: " + unit.Health);
    }

    public AIAction GetRandomMove(GameState state, User player)
    {
        var actions = GetLegalActions(state, player);
        var ran = random.Next(actions.Count);
        return actions[ran];
    }

    void Debug(List<AIAction> list)
    {
        int attackActions = 0, moveActions = 0, createActions = 0;

        foreach (AIAction a in list)
        {
            if (a is AIAttackAction)
                attackActions++;
            if (a is AIMoveAction)
                moveActions++;
            if (a is AICreateAction b)
                createActions++;
        }
        Godot.Logging.Log("Attack Actions: " + attackActions);
        Godot.Logging.Log("Move Actions: " + moveActions);
        Godot.Logging.Log("Create Actions: " + createActions);
    }

    public static void DebugAction(AIAction action)
    {
        string str = "";
        if (action is AIAttackAction a)
            str = a.attacker.UnitType + " at " + a.attacker.X + ", " + a.attacker.Y + " attacks " + a.defender.UnitType + " at " + a.defender.X + ", " + a.defender.Y;
        if (action is AIMoveAction b)
            str = b.unit.UnitType + " at " + b.unit.X + ", " + b.unit.Y + " moves to " + b.x + ", " + b.y;
        if (action is AICreateAction c)
            str = c.unitType + " is created at " + c.x + ", " + c.y;
        Godot.Logging.Log(str);
    }
}