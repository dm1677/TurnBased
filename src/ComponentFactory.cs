using System;
using static GameSystem;

public partial class ComponentFactory
{
    static ComponentFactory componentFactory;

    public static ComponentFactory Instance()
    {
        if (componentFactory == null) { componentFactory = new ComponentFactory(); GetValuesFromFile(); }
        return componentFactory;
    } 

    public Entity CreateUnit(int x, int y, Unit unit, User owner)
    {
        switch(unit)
        {
            case Unit.Prawn:
                return CreatePrawn(x, y, owner);
            case Unit.Building:
                return CreateStatue(x, y, owner);
            case Unit.King:
                return CreateKing(x, y, owner);
            case Unit.Knight:
                return CreateKnight(x, y, owner);
            case Unit.Gobbo:
                return CreateGobbo(x, y, owner);
            case Unit.Tree:
                return CreateTree(x, y);
        }
        throw new ArgumentException("CreateUnit: No matching unit type");
    }

    public Entity CreateTree(int x, int y)
    {
        Entity entity = GameSystem.EntityManager.CreateEntity();

        GameSystem.EntityManager.AddComponent(entity, new Position() { X = x, Y = y });
        GameSystem.EntityManager.AddComponent(entity, new Sprite() { path = "res://assets/sprites/tree.png" });
        GameSystem.EntityManager.AddComponent(entity, new Owner() { ownedBy = User.Neutral });
        GameSystem.EntityManager.AddComponent(entity, new Name() { name = "Tree" });

        return entity;
    }

    public Entity CreatePrawn(int x, int y, User owner)
    {
        Entity entity = prawn.Create(x, y);
        prawn.AddMovement(entity);
        GameSystem.EntityManager.AddComponent(entity, new Swap());
        GameSystem.EntityManager.AddComponent(entity, new Owner() { ownedBy = owner });

        return entity;
    }

    public Entity CreateKing(int x, int y, User owner)
    {
        Entity entity = king.Create(x, y);
        king.AddMovement(entity);
        GameSystem.EntityManager.AddComponent(entity, new Owner() { ownedBy = owner });

        return entity;
    }

    public Entity CreateKnight(int x, int y, User owner)
    {
        Entity entity = knight.Create(x, y);
        knight.AddMovement(entity);
        GameSystem.EntityManager.AddComponent(entity, new Owner() { ownedBy = owner });

        return entity;
    }

    public Entity CreateStatue(int x, int y, User owner)
    {
        Entity entity = statue.Create(x, y);
        GameSystem.EntityManager.AddComponent(entity, new Owner() { ownedBy = owner });

        return entity;
    }

    public Entity CreateGobbo(int x, int y, User owner)
    {
        Entity entity = gobbo.Create(x, y);
        gobbo.AddMovement(entity);
        GameSystem.EntityManager.AddComponent(entity, new Owner() { ownedBy = owner });

        return entity;
    }

    public Entity CreateResource(int ownerID)
    {
        Entity entity = GameSystem.EntityManager.CreateEntity();

        GameSystem.EntityManager.AddComponent(entity, new Name() { name = "Money" });//temp??
        GameSystem.EntityManager.AddComponent(entity, new GResource() { Name = "Money", Value = money.Cost });
        GameSystem.EntityManager.AddComponent(entity, new Owner() { ownedBy = (User)ownerID });

        return entity;
    }

    public Entity CreateTimer(TimerType timerType, int totalTime, int increment, int ownerID)
    {
        Entity entity = GameSystem.EntityManager.CreateEntity();

        GameSystem.EntityManager.AddComponent(entity, new Timer(timerType, totalTime, increment));
        GameSystem.EntityManager.AddComponent(entity, new Owner() { ownedBy = (User)ownerID });

        return entity;
    }
}