using System.Collections.Generic;
using static GameSystem;

public class HealthHandler : IHandler
{
    readonly HashSet<Health> healthComponentList = new HashSet<Health>();

    public bool Process()
    {
        RefreshComponentList();
        ProcessHealth();
        return true;
    }

    public void Reverse()
    {
        RefreshComponentList();
        foreach (Health health in healthComponentList)
        {
            ReverseRegeneration(health);
            CheckHealthBetweenZeroAndMax(health);
        }
    }

    void RefreshComponentList()
    {
        var keys = GameSystem.EntityManager.GetEntityList().Keys;

        healthComponentList.Clear();

        foreach (Entity entity in keys)
        {
            Health health = GameSystem.EntityManager.GetComponent<Health>(entity);
            if (health != null)
                healthComponentList.Add(health);
        }
    }

    void UpdateComponentList()
    {
        var keys = GameSystem.EntityManager.GetEntityList().Keys;

        foreach (Entity entity in keys)
        {
            Health health = GameSystem.EntityManager.GetComponent<Health>(entity);
            if (health != null) healthComponentList.Add(health);
        }
    }

    void ProcessHealth()
    {
        foreach (Health health in healthComponentList)
        {
            ProcessRegeneration(health);
            CheckHealthBetweenZeroAndMax(health);
            CheckRemoveUnit(health);
        }
    }

    void CheckHealthBetweenZeroAndMax(Health health)
    {
        if (health.CurrentHP > health.MaxHP)
            health.CurrentHP = health.MaxHP;
        if (health.CurrentHP < 0)
            health.CurrentHP = 0;
    }

    void ProcessRegeneration(Health health)
    {
        if (GameSystem.Game.Turn.CheckEntityOwnedByActivePlayer(health.Parent))
        {
            if (health.CurrentHP > 0 && health.CurrentHP < health.MaxHP)
            {
                health.CurrentHP += health.Regeneration;
                health.TurnsSinceRegeneration = 0;
            } else health.TurnsSinceRegeneration++;
        }
    }

    void ReverseRegeneration(Health health)
    {
        if (!GameSystem.Game.Turn.CheckEntityOwnedByActivePlayer(health.Parent))
        {
            if (health.TurnsSinceRegeneration == 0)
                health.CurrentHP -= health.Regeneration;
            else health.TurnsSinceRegeneration--;
        }
    }

    bool CheckRemoveUnit(Health health)
    {
        if (health.CurrentHP <= 0)
        {
            if (Input.GetSelection() == health.Parent)
                Input.SetNullSelection();

            GameSystem.EntityManager.QueueForDeletion(health.Parent);
            CheckEndGame(health.Parent);
            return true;
        }

        return false;
    }

    void CheckEndGame(Entity entity)
    {
        if (UnitIsKing(entity))
            KingDead(entity);
    }

    //temporary - can put in own class if needed in future
    bool UnitIsKing(Entity entity)
    {
        Name name = GameSystem.EntityManager.GetComponent<Name>(entity);

        if (name != null && name.name == "King")
            return true;
        else return false;
    }

    int KingCount(int playerID)
    {
        int count = 0;
        var entityList = GameSystem.EntityManager.GetEntityList().Keys;

        foreach (Entity entity in entityList)
        {
            //Enemy units
            List<Component> list = GameSystem.EntityManager.GetComponentList(entity);
            Name name = GameSystem.EntityManager.GetComponent<Name>(list);
            Owner owner = GameSystem.EntityManager.GetComponent<Owner>(list);

            if (name != null && owner != null
             && name.name == "King" && owner.ownedBy == (User)playerID)
            {
                count++;
            }
        }
        Godot.GD.Print(count);
        return count;
    }

    //temporary - can put in own class if needed in future
    void KingDead(Entity entity)
    {
        Owner owner = GameSystem.EntityManager.GetComponent<Owner>(entity);

        if (owner != null && KingCount((int)owner.ownedBy) <= 1)
        {
            if (owner.ownedBy == (User)GameSystem.Player.GetID())
                GameSystem.Game.Rpc("GameResult", Enemy.GetName());
            //else if (owner.ownedBy == (Owner.Player)enemy.GetID())
            //    game.Rpc("GameResult", player.GetName());
        }
    }

}