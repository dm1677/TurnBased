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

    public Entity CreateUnit(int x, int y, Unit unit)
    {
        switch(unit)
        {
            case Unit.Prawn:
                return CreatePrawn(x, y);
            case Unit.Building:
                return CreateStatue(x, y);
            case Unit.King:
                return CreateKing(x, y);
            case Unit.Knight:
                return CreateKnight(x, y);
            case Unit.Gobbo:
                return CreateGobbo(x, y);
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

    public Entity CreatePrawn(int x, int y)
    {
        Entity entity = prawn.Create(x, y);
        prawn.AddMovement(entity);
        GameSystem.EntityManager.AddComponent(entity, new Swap());

        return entity;
    }

    public Entity CreateKing(int x, int y)
    {
        Entity entity = king.Create(x, y);
        king.AddMovement(entity);

        return entity;
    }

    public Entity CreateKnight(int x, int y)
    {
        Entity entity = knight.Create(x, y);
        knight.AddMovement(entity);

        return entity;
    }

    public Entity CreateStatue(int x, int y)
    {
        Entity entity = statue.Create(x, y);

        return entity;
    }

    public Entity CreateGobbo(int x, int y)
    {
        Entity entity = gobbo.Create(x, y);
        gobbo.AddMovement(entity);

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