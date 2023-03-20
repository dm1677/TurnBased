using System.Collections.Generic;

public class AIAttackAction : AIAction
{
    public UnitState attacker, defender;

    public AIAttackAction(UnitState attacker, UnitState defender)
    {
        this.attacker = attacker;
        this.defender = defender;
    }

    public static bool UnitIsKilled(UnitState attacker, UnitState defender)
    {
        int damage = 0;

        if (defender.Health < attacker.UnitClass.Damage) damage = defender.Health;
        else damage = attacker.UnitClass.Damage;

        if (defender.Health - damage <= 0)
            return true;
        else
            return false;
    }

    public static List<UnitState> Attack(AttackType attackType, GameState gameState, UnitState unitState)
    {
        switch (attackType)
        {
            case AttackType.Line:
                return LineAttack(gameState, unitState);
            case AttackType.Diagonal:
                return DiagonalAttack(gameState, unitState);
            case AttackType.LineAndDiagonal:
                var lineAttack = LineAttack(gameState, unitState);
                var diagonalAttack = DiagonalAttack(gameState, unitState);
                lineAttack.AddRange(diagonalAttack);
                return lineAttack;
        }
        return new List<UnitState>();
    }

    static List<UnitState> LineAttack(GameState gameState, UnitState unit)
    {
        int i;

        List<UnitState> enemyUnitCollisions = new List<UnitState>();

        int range = unit.UnitClass.Range;

        var enemyUnits = gameState.GetEnemyUnits(unit.Owner);

        for (int x = 0; x <= range; x++)
        {
            i = unit.X + x;

            if (i > gameState.mapWidth - 1) break;
            foreach (UnitState enemyUnit in enemyUnits)
            {
                if (enemyUnit.X == i && enemyUnit.Y == unit.Y)
                {
                    enemyUnitCollisions.Add(enemyUnit);
                    goto A;
                }
            }

            if (i != unit.X && !gameState.Passable(i, unit.Y)) goto A;
        }

    A:
        for (int x = 0; x >= -range; x--)
        {
            i = unit.X + x;

            if (i < 0) break;

            foreach (UnitState enemyUnit in enemyUnits)
            {
                if (enemyUnit.X == i && enemyUnit.Y == unit.Y)
                {
                    enemyUnitCollisions.Add(enemyUnit);
                    goto B;
                }
            }

            if (i != unit.X && !gameState.Passable(i, unit.Y)) goto B;
        }

    B:
        for (int y = 0; y <= range; y++)
        {
            i = unit.Y + y;

            if (i > gameState.mapHeight - 1) break;

            foreach (UnitState enemyUnit in enemyUnits)
            { 
                if (enemyUnit.Y == i && enemyUnit.X == unit.X)
                {
                    enemyUnitCollisions.Add(enemyUnit);
                    goto C;
                }
            }

            if (i != unit.Y && !gameState.Passable(unit.X, i)) goto C;
        }

    C:
        for (int y = 0; y >= -range; y--)
        {
            i = unit.Y + y;

            if (i < 0) break;
            foreach (UnitState enemyUnit in enemyUnits)
            {
                if (enemyUnit.Y == i && enemyUnit.X == unit.X)
                {
                    enemyUnitCollisions.Add(enemyUnit);
                    goto D;
                }
            }

            if (i != unit.Y && !gameState.Passable(unit.X, i)) break;
        }

    D:
        return enemyUnitCollisions;
    }

    static List<UnitState> DiagonalAttack(GameState gameState, UnitState unit)
    {
        int i, x, y;
        int width = gameState.mapWidth;
        int height = gameState.mapHeight;

        List<UnitState> enemyUnitCollisions = new List<UnitState>();

        var range = unit.UnitClass.Range;
        var enemyUnits = gameState.GetEnemyUnits(unit.Owner);

        for (i = 0; i <= range; i++)
        {
            x = unit.X + i;
            y = unit.Y - i;

            //Make sure we're not checking a tile outside of the bounds of the map array
            if (!IsInMapLimits(x, y, width, height)) break;

            foreach (UnitState enemyUnit in enemyUnits)
            {
                if (enemyUnit.X == x && enemyUnit.Y == y)
                {
                    enemyUnitCollisions.Add(enemyUnit);
                    goto A;
                }
            }
            //If we're checking an impassable tile (not including the tile our current selection is standing on), don't allow this loop to continue
            //-this way we can't jump over impassable tiles
            if (x != unit.X && y != unit.Y && !gameState.Passable(x, y)) goto A;
        }
    A:
        for (i = 0; i <= range; i++)
        {
            x = unit.X + i;
            y = unit.Y + i;

            if (!IsInMapLimits(x, y, width, height)) break;

            foreach (UnitState enemyUnit in enemyUnits)
            {
                if (enemyUnit.X == x && enemyUnit.Y == y)
                {
                    enemyUnitCollisions.Add(enemyUnit);
                    goto B;
                }
            }

            if (x != unit.X && y != unit.Y && !gameState.Passable(x, y)) goto B;
        }


    B:
        for (i = 0; i <= range; i++)
        {
            x = unit.X - i;
            y = unit.Y + i;

            if (!IsInMapLimits(x, y, width, height)) break;

            foreach (UnitState enemyUnit in enemyUnits)
            {
                if (enemyUnit.X == x && enemyUnit.Y == y)
                {
                    enemyUnitCollisions.Add(enemyUnit);
                    goto C;
                }
            }

            if (x != unit.X && y != unit.Y && !gameState.Passable(x, y)) goto C;
        }

    C:
        for (i = 0; i <= range; i++)
        {
            x = unit.X - i;
            y = unit.Y - i;

            if (!IsInMapLimits(x, y, width, height)) break;

            foreach (UnitState enemyUnit in enemyUnits)
            {
                if (enemyUnit.X == x && enemyUnit.Y == y)
                {
                    enemyUnitCollisions.Add(enemyUnit);
                    goto D;
                }
            }

            if (x != unit.X && y != unit.Y && !gameState.Passable(x, y)) break;
        }
    D:
        return enemyUnitCollisions;
    }

    static bool IsInMapLimits(int x, int y, int width, int height)
    {
        width--;
        height--;

        if (x < 0
            || y < 0
            || y > height
            || x > width
            )
            return false;

        else return true;
    }
}