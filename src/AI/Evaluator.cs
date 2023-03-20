using System;
using System.Collections.Generic;

public static class Evaluator
{
    public static float UnitHealth(GameState state, User player)
    {
        float playerHealthScore = 0;
        float enemyHealthScore = 0;
        int playerUnitCount = 0;
        int enemyUnitCount = 0;

        foreach (UnitState unit in state.units)
        {
            if (unit.UnitType == Unit.King || unit.UnitType == Unit.Resource || unit.UnitType == Unit.Tree) continue;

            if (unit.Owner == player) playerUnitCount++;
            else enemyUnitCount++;

            float h = unit.Health / unit.UnitClass.MaxHP;
            if (unit.Owner == player) playerHealthScore += h;
            else enemyHealthScore += h;
        }

        if (playerUnitCount != 0)
            playerHealthScore /= playerUnitCount;
        if (enemyUnitCount != 0)
            enemyHealthScore /= enemyUnitCount;

        return (playerHealthScore - enemyHealthScore);
    }

    public static int Locality(GameState state, User player, List<UnitState> kings)
    {
        int playerLocality = 0;
        int enemyLocality = 0;
        int playerUnitCount = 0;
        int enemyUnitCount = 0;

        foreach (UnitState unit in state.units)
        {
            if (unit.UnitType == Unit.Resource || unit.UnitType == Unit.Tree || unit.UnitType == Unit.King) continue;

            if (unit.Owner == player) playerUnitCount++;
            else enemyUnitCount++;

            foreach (UnitState king in kings)
            {
                int distance = (int)(Math.Sqrt(Math.Pow((unit.X - king.X), 2) + Math.Pow((unit.Y - king.Y), 2)));

                if (unit.Owner == player && king.Owner != player)
                    playerLocality += Math.Min(playerLocality, distance) * (1 / (unit.UnitClass.Speed + 1));
                else if (unit.Owner != player && king.Owner == player)
                    enemyLocality += Math.Min(playerLocality, distance) * (1 / (unit.UnitClass.Speed + 1));
            }
        }

        if (playerUnitCount == 0 || enemyUnitCount == 0) return 0;

        playerLocality /= playerUnitCount;
        enemyLocality /= enemyUnitCount;

        return enemyLocality - playerLocality;
    }

    public static float Mobility(GameState state, User player)
    {
        float myMobility = 0;
        float enemyMobility = 0;

        int myMoves = 0;
        int enemyMoves = 0;

        foreach (UnitState unit in state.units)
        {
            if (unit.UnitType == Unit.Resource || unit.UnitType == Unit.Tree || unit.UnitType == Unit.King) continue;
            if (unit.UnitClass.Speed > 0)
            {
                for (int y = 0; y < state.mapHeight; y++)
                {
                    for (int x = 0; x < state.mapWidth; x++)
                    {
                        if (state.Passable(x, y) && AIMoveAction.CheckMovement(unit.UnitClass.MovementType, state, unit, x, y))
                            if (unit.Owner == player) myMoves++;
                            else enemyMoves++;
                    }
                }
            }
        }

        myMobility = myMoves * 0.1f;
        enemyMobility = enemyMoves * 0.1f;

        return (myMobility - enemyMobility);
    }

    public static int Threats(GameState state, User player)
    {
        int myThreatScore = 0;
        int enemyThreatScore = 0;

        int myBestHitsToKill = int.MaxValue;
        int enemyBestHitsToKill = int.MaxValue;

        foreach (UnitState unit in state.units)
        {
            if (unit.UnitType == Unit.Resource || unit.UnitType == Unit.Tree || unit.UnitType == Unit.King) continue;

            var list = AIAttackAction.Attack(unit.UnitClass.AttackType, state, unit);
            if (unit.Owner == player)
            {
                if (unit.UnitType == Unit.Building)
                    myThreatScore += list.Count * 3;
                else if (unit.UnitType == Unit.Knight)
                    myThreatScore += list.Count * 4;
                myThreatScore += list.Count;
            }
            else
            {
                if (unit.UnitType == Unit.Building)
                    enemyThreatScore += list.Count * 3;
                else if (unit.UnitType == Unit.Knight)
                    enemyThreatScore += list.Count * 4;
                enemyThreatScore += list.Count;
            }

            foreach (UnitState attackedUnit in list)
            {
                if (attackedUnit.UnitType == Unit.King)
                {
                    int x = attackedUnit.Health;
                    int y = unit.UnitClass.Damage;
                    int z = (x + (y - 1)) / y;

                    if (unit.Owner == player)
                        myBestHitsToKill = Math.Min(myBestHitsToKill, z);
                    else enemyBestHitsToKill = Math.Min(enemyBestHitsToKill, z);
                }
            }
        }

        if (myBestHitsToKill == int.MaxValue || enemyBestHitsToKill == int.MaxValue) return (myThreatScore - enemyThreatScore);

        if (myBestHitsToKill <= enemyBestHitsToKill) myThreatScore += 50;
        else enemyThreatScore += 50;

        return (myThreatScore - enemyThreatScore);
    }

    public static int UnitCost(GameState state, User player)
    {
        int playerEvaluation = 0;
        int enemyEvaluation = 0;

        foreach (UnitState unit in state.units)
        {
            if (unit.UnitType == Unit.Tree || unit.UnitType == Unit.Resource || unit.UnitType == Unit.King) continue;

            if (unit.Owner == player)
            {
                if (unit.UnitType == Unit.Gobbo)//just for gobbo bot
                    playerEvaluation += 3;
                playerEvaluation += (unit.UnitClass.Cost * 3) + 2;
                if (unit.UnitType == Unit.Knight)
                    playerEvaluation += 10;
                if (unit.UnitType == Unit.Building)
                    playerEvaluation += 14;
            }
            else
            {
                if (unit.UnitType == Unit.Gobbo)//just for gobbo bot
                    enemyEvaluation += 3;
                enemyEvaluation += (unit.UnitClass.Cost * 3) + 2;
                if (unit.UnitType == Unit.Knight)
                    enemyEvaluation += 10;
                if (unit.UnitType == Unit.Building)
                    enemyEvaluation += 14;
            }
        }

        return playerEvaluation - enemyEvaluation;
    }

    public static int UnitS(GameState state, User player)
    {
        int playerUnitS = 0;
        int enemyUnitS = 0;

        foreach (UnitState unit in state.units)
        {
            if (unit.UnitClass == null) continue;
            if (unit == null) continue;
            var list = AIAttackAction.Attack(unit.UnitClass.AttackType, state, unit);
            if (list.Count == 0) continue;
            foreach (UnitState u in list)
            {
                if (unit.Owner == player)
                {
                    playerUnitS += 2;
                    if (u.UnitType == Unit.King)
                        playerUnitS += 5;
                    if (unit.UnitType == u.UnitType)
                    {
                        if ((unit.Health - unit.UnitClass.Damage) >= u.Health)
                            playerUnitS += 8;
                        else
                            playerUnitS -= 8;
                    }
                }
                else
                {
                    enemyUnitS += 2;
                    if (u.UnitType == Unit.King)
                        enemyUnitS += 5;
                    if (unit.UnitType == u.UnitType)
                    {
                        if ((unit.Health - unit.UnitClass.Damage) >= u.Health)
                            enemyUnitS += 8;
                        else
                            enemyUnitS -= 8;
                    }
                }
            }
        }

        return (playerUnitS - enemyUnitS);
    }

    public static int KingHealthPool(GameState state, User player)
    {
        int playerKingHealth = 0;
        int enemyKingHealth = 0;
        int playerKings = 0;
        int enemyKings = 0;

        foreach (UnitState unit in state.units)
        {
            if (unit.UnitType == Unit.King)
            {
                if (unit.Owner == player)
                {
                    playerKings++;
                    playerKingHealth += unit.Health;
                }
                else
                {
                    enemyKings++;
                    enemyKingHealth += unit.Health;
                }
            }
        }

        if (playerKings == 0) playerKings = -100;
        if (enemyKings == 0) enemyKings = -100;

        //return 2*(playerKingHealth + (20*playerKings)) - (enemyKingHealth + (20*enemyKings));
        return playerKingHealth - enemyKingHealth;
    }

    public static int KingTrap(GameState state, User player, List<UnitState> kings, int playerMoney, int enemyMoney)
    {
        int playerScore = 0;
        int enemyScore = 0;

        foreach (UnitState king in kings)
        {
            if (king.X == 0 || king.X == 14)
            {
                if (king.Owner == player)
                    playerScore += 5;
                else enemyScore += 5;
            }
            if (king.Y == 0 || king.Y == 14)
            {
                if (king.Owner == player)
                    playerScore += 5;
                else enemyScore += 5;
            }
            if (king.X == 1 || king.X == 13)
            {
                if (king.Owner == player)
                    playerScore += 5;
                else enemyScore += 5;
            }
            if (king.Y == 1 || king.Y == 13)
            {
                if (king.Owner == player)
                    playerScore += 5;
                else enemyScore += 5;
            }

            if (playerMoney >= 9) enemyScore *= 2;
            if (enemyMoney >= 9) playerScore *= 2;

            if (king.X == 0 && !state.Passable(king.X + 1, king.Y))
            {
                if (king.Owner == player)
                    playerScore += 10;
                else enemyScore += 10;
            }
            if (king.X == 14 && !state.Passable(king.X - 1, king.Y))
            {
                if (king.Owner == player)
                    playerScore += 10;
                else enemyScore += 10;
            }
            if (king.Y == 14 && !state.Passable(king.X, king.Y - 1))
            {
                if (king.Owner == player)
                    playerScore += 10;
                else enemyScore += 10;
            }
            if (king.Y == 0 && !state.Passable(king.X, king.Y + 1))
            {
                if (king.Owner == player)
                    playerScore += 10;
                else enemyScore += 10;
            }

            foreach(UnitState king2 in kings)
            {
                if (king2 == king) continue;
                if (king2.Health > king.UnitClass.Damage && king2.Health > king.Health)
                {
                    if ((king2.X == king.X + 1 && king2.Y == king.Y) || (king2.X == king.X - 1 && king2.Y == king.Y) || (king2.Y == king.Y + 1 && king2.X == king.X) || (king2.Y == king.Y - 1 && king2.X == king.X))
                    {
                        if (king.Owner == player)
                            playerScore += 20;
                        else enemyScore += 20;
                    }
                }
            }
        }

        return enemyScore - playerScore;
    }

    public static int KingSafety(GameState state, User player, int playerMoney, int enemyMoney)
    {
        int playerKingSafety = 0;
        int enemyKingSafety = 0;

        foreach (UnitState unit in state.units)
        {
            if (unit.UnitType == Unit.King)
            {
                if (((unit.X >= 0 || unit.X <= 2) && (unit.Y >= 0 || unit.Y <= 2))
                    || ((unit.X >= 0 || unit.X <= 2) && (unit.Y >= 12 || unit.Y <= 14))
                    || ((unit.X >= 12 || unit.X <= 14) && (unit.Y >= 0 || unit.Y <= 2))
                    || ((unit.X >= 12 || unit.X <= 14) && (unit.Y >= 12 || unit.Y <= 14)))
                {
                    if (unit.Owner == player)
                    {
                        if (enemyMoney >= 9)
                            playerKingSafety += 20;
                    }
                    else
                    {
                        if (playerMoney >= 9)
                            enemyKingSafety += 20;
                    }
                }
                if ((unit.X >= 0 && unit.X <= 2) || (unit.X >= 12 && unit.X <= 14) || (unit.Y >= 0 && unit.Y <= 2) || (unit.Y >= 12 && unit.Y <= 14))
                {
                    if (unit.Owner == player)
                        playerKingSafety += 2;
                    else
                        enemyKingSafety += 2;
                }
            }

        }

        return (enemyKingSafety - playerKingSafety);
    }

    public static int KingThing(GameState state, User player)
    {
        int playerKingHealth = 0;

        foreach (UnitState unit in state.units)
        {
            if (unit.UnitType == Unit.King)
            {
                if (unit.Owner == player)
                    playerKingHealth += unit.Health;
            }
        }

        return playerKingHealth;
    }

    public static int Resources(int myMoney, int enemyMoney)
    {
        return myMoney - enemyMoney;
    }

    public static float Units(GameState state, User player)
    {
        float playerEvaluation = 0;
        float enemyEvaluation = 0;

        foreach (UnitState unit in state.units)
        {
            if (unit.UnitType == Unit.Tree || unit.UnitType == Unit.Resource || unit.UnitType == Unit.King) continue;

            if (unit.Owner == player)
            {
                playerEvaluation += unit.UnitClass.Cost;
                playerEvaluation += (unit.Health / unit.UnitClass.MaxHP);
            }
            else
            {
                enemyEvaluation += unit.UnitClass.Cost;
                enemyEvaluation += (unit.Health / unit.UnitClass.MaxHP);
            }
        }

        return playerEvaluation - enemyEvaluation;
    }

    public static int KingCount(GameState state, User player)
    {
        int myCount = 0;
        int enemyCount = 0;

        foreach (UnitState unit in state.units)
        {
            if (unit.UnitType != Unit.King) continue;
            if (unit.Owner == player)
                myCount++;
            else enemyCount++;
        }

        return (myCount - enemyCount);
    }

    public static int KingCrowding(User player, List<UnitState> kings)
    {
        List<UnitState> playerKings = new List<UnitState>(2);
        int score = 0;

        foreach (UnitState king in kings)
        {
            if (king.Owner == player)
                playerKings.Add(king);
        }

        if (playerKings.Count > 1)
        {
            score += (int)(Math.Sqrt(Math.Pow((playerKings[0].X - playerKings[1].X), 2) + Math.Pow((playerKings[0].Y - playerKings[1].Y), 2)));
        }

        return score;
    }

    public static int Race(GameState state, User player, List<UnitState> kings)
    {
        List<UnitState> playerKings = new List<UnitState>(2);
        List<UnitState> enemyKings = new List<UnitState>(2);

        foreach (UnitState king in kings)
        {
            if (king.Owner == player)
                playerKings.Add(king);
            else
                enemyKings.Add(king);
        }

        int highestAttackAgainstMe = 0;
        int highestAttackAgainstEnemy = 0;

        foreach (UnitState unit in state.units)
        {
            if (unit.UnitType == Unit.Resource || unit.UnitType == Unit.Tree) continue;
            if (unit.Owner != player)
            {
                var attack = AIAttackAction.Attack(unit.UnitClass.AttackType, state, unit);
                if(attack.Count > 0)
                {
                    foreach(UnitState king in playerKings)
                    {
                        if (attack.Contains(king))
                            highestAttackAgainstMe = Math.Max(unit.UnitClass.Damage, highestAttackAgainstMe);
                    }
                }
            }
            else
            {
                var attack = AIAttackAction.Attack(unit.UnitClass.AttackType, state, unit);
                if (attack.Count > 0)
                {
                    foreach (UnitState king in playerKings)
                    {
                        if (attack.Contains(king))
                            highestAttackAgainstEnemy = Math.Max(unit.UnitClass.Damage, highestAttackAgainstEnemy);
                    }
                }
            }
        }

        if(highestAttackAgainstMe != 0)
        {
            if (highestAttackAgainstEnemy == 0) return -1;
            if (highestAttackAgainstMe > highestAttackAgainstEnemy) return -1;
            if (highestAttackAgainstMe <= highestAttackAgainstEnemy) return 1;
        }

        return 0;
    }
}