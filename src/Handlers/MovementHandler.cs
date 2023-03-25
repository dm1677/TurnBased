using Godot;
using System.Collections.Generic;
using static GameSystem;

public class MovementHandler : IHandler
{
    readonly HashSet<Movement> movementComponentList = new HashSet<Movement>();

    public bool Process(Action action)
    {
        if (action == null) return false;
        UpdateComponentList();
        CheckEnableKingMovement();

        if (action is MoveAction moveAction) return IsValidMoveAction(moveAction);
        if (action is SwapAction swapAction) return IsValidSwapAction(swapAction);
        if (action is AttackAction attackAction) return IsValidAttackAction(attackAction);
        return true;
    }

    public void Reverse() { }

    void UpdateComponentList()
    {
        var keys = GameSystem.EntityManager.GetEntityList().Keys;
        movementComponentList.Clear();
        foreach (Entity entity in keys)
        {
            Movement movement = GameSystem.EntityManager.GetComponent<Movement>(entity);
            if (movement != null) movementComponentList.Add(movement);
        }
    }

    bool IsValidMoveAction(MoveAction moveAction)
    {
        Entity entity = GameSystem.EntityManager.GetEntity(moveAction.EntityID);
        Coords destination = new Coords(moveAction.DestinationX, moveAction.DestinationY);

        Position position = GameSystem.EntityManager.GetComponent<Position>(entity);
        Movement movement = GameSystem.EntityManager.GetComponent<Movement>(entity);

        if (!GameSystem.Game.Turn.CheckEntityOwnedByActivePlayer(entity)) return false;
        if (!GameSystem.Map.IsPassable(destination)) return false;

        var enemyUnit = CheckNearbyEnemies(entity, destination);
        if (enemyUnit == null)
            return CheckMovement(destination, position, movement);
        return true;
    }

    bool IsValidSwapAction(SwapAction swapAction)
    {
        if (swapAction.SwappedEntity == swapAction.SwappingEntity) return false;
        Swap swap = GameSystem.EntityManager.GetComponent<Swap>(swapAction.SwappingEntity);
        Swap swap2 = GameSystem.EntityManager.GetComponent<Swap>(swapAction.SwappedEntity);

        Position position = GameSystem.EntityManager.GetComponent<Position>(swapAction.SwappingEntity);
        Position position2 = GameSystem.EntityManager.GetComponent<Position>(swapAction.SwappedEntity);

        Owner owner = GameSystem.EntityManager.GetComponent<Owner>(swapAction.SwappingEntity);
        Owner owner2 = GameSystem.EntityManager.GetComponent<Owner>(swapAction.SwappedEntity);

        if (position == null || position2 == null || owner == null || owner2 == null) return false;
        if (swap == null && swap2 == null) return false;
        if (owner.ownedBy != owner2.ownedBy) return false;
        if (!GameSystem.Game.Turn.CheckEntityOwnedByActivePlayer(GameSystem.EntityManager.GetEntity(swapAction.SwappingEntity))) return false;
        return true;
    }

    bool IsValidAttackAction(AttackAction attackAction)
    {
        Entity attacker = GameSystem.EntityManager.GetEntity(attackAction.AttackerID);
        Weapon weapon = GameSystem.EntityManager.GetComponent<Weapon>(attacker);
        List<Entity> attackedEntities = Attack(attacker, weapon.attackType);
        foreach (Entity e in attackedEntities)
            Godot.GD.Print("Attacked entity ID: " + e.ID);

        Entity defender = GameSystem.EntityManager.GetEntity(attackAction.DefenderID);
        var a = attackedEntities.Contains(defender);
        Godot.GD.Print("Defender's ID: " + defender.ID);
        Godot.GD.Print("Attack action accepted: " + a);
        return a;
    }

    Entity CheckNearbyEnemies(Entity entity, Coords destination)
    {
        Weapon weapon = GameSystem.EntityManager.GetComponent<Weapon>(entity);
        var enemyUnits = Attack(entity, weapon.attackType);

        if (enemyUnits.Count > 0)
        {
            foreach (Entity enemyUnit in enemyUnits)
            {
                Position enemyPosition = GameSystem.EntityManager.GetComponent<Position>(enemyUnit);
                if (enemyPosition.X == destination.X && enemyPosition.Y == destination.Y)
                    return enemyUnit;
            }
        }

        return null;
    }

    public static bool CheckMovement(Coords destination, Coords currentPosition, int speed, MovementType movementType)
    {
        if (currentPosition.X == destination.X && currentPosition.Y == destination.Y) return false;

        return Movement(destination, currentPosition, speed, movementType);
    }

    public static bool CheckMovement(Coords destination, Position position, Movement movement)
    {
        if (position == null || movement == null) return false;
        if (position.X == destination.X && position.Y == destination.Y) return false;

        var currentPosition = new Coords(position.X, position.Y);
        var movementType = movement.movementType;
        var speed = movement.speed;

        return Movement(destination, currentPosition, speed, movementType);
    }

    public static bool CheckMovement(Entity destinationEntity, Entity movingEntity)
    {
        Position position = GameSystem.EntityManager.GetComponent<Position>(movingEntity);
        Movement movement = GameSystem.EntityManager.GetComponent<Movement>(movingEntity);

        Position destinationPositionComponent = GameSystem.EntityManager.GetComponent<Position>(destinationEntity);
        var destination = new Coords(destinationPositionComponent.X, destinationPositionComponent.Y);

        if (position == null || movement == null) return false;
        if (position.X == destination.X && position.Y == destination.Y) return false;

        var currentPosition = new Coords(position.X, position.Y);
        var movementType = movement.movementType;
        var speed = movement.speed;

        return Movement(destination, currentPosition, speed, movementType);
    }    

    public static bool Movement(Coords destination, Coords currentPosition, int speed, MovementType direction)
    {
        List<(int, int)> vectors = new List<(int, int)>(0);

        if (direction == MovementType.Line)             vectors = new List<(int, int)> { (0, 1), (1, 0), (-1, 0), (0, -1) };
        if (direction == MovementType.Diagonal)         vectors = new List<(int, int)> { (1, 1), (-1, -1), (-1, 1), (1, -1) };
        if (direction == MovementType.LineAndDiagonal)  vectors = new List<(int, int)> { (0, 1), (1, 0), (-1, 0), (0, -1), (1, 1), (-1, -1), (-1, 1), (1, -1) };

        int x, y;
        
        for (int k = 0; k < vectors.Count; k++)
        {
            for (int i = 1; i <= speed; i++)
            {
                x = currentPosition.X + (i * vectors[k].Item1);
                y = currentPosition.Y + (i * vectors[k].Item2);

                if (!GameSystem.Map.IsInBounds(x, y) || !GameSystem.Map.IsPassable(x, y)) break;

                if (destination.X == x && destination.Y == y) return true;
            }
        }

        return false;
    }

    public static List<Entity> Attack(Entity entity, AttackType direction)
    {
        List<Entity> enemyUnitCollisions = new List<Entity>();
        if (entity == null) return enemyUnitCollisions;

        var selectionList = GameSystem.EntityManager.GetComponentList(entity);

        Owner owner = GameSystem.EntityManager.GetComponent<Owner>(entity);
        if (owner == null) return enemyUnitCollisions;
        var ownedBy = (int)owner.ownedBy;

        Weapon weapon = GameSystem.EntityManager.GetComponent<Weapon>(selectionList);
        int range = 0;
        if (weapon != null)
        {
            range = weapon.range;
        }

        Position selectionPosition = GameSystem.EntityManager.GetComponent<Position>(selectionList);

        List<Position> enemyPositions = new List<Position>();

        foreach (Entity enemyUnit in GameSystem.EntityManager.GetEnemyUnits(ownedBy))
        {
            Position pos = GameSystem.EntityManager.GetComponent<Position>(enemyUnit);
            if (pos != null) enemyPositions.Add(pos);
        }

        List<(int, int)> vectors = new List<(int, int)>(0);

        if (direction == AttackType.Line) vectors = new List<(int, int)> { (0, 1), (1, 0), (-1, 0), (0, -1) };
        if (direction == AttackType.Diagonal) vectors = new List<(int, int)> { (1, 1), (-1, -1), (-1, 1), (1, -1) };
        if (direction == AttackType.LineAndDiagonal) vectors = new List<(int, int)> { (0, 1), (1, 0), (-1, 0), (0, -1), (1, 1), (-1, -1), (-1, 1), (1, -1) };

        int x, y;

        for (int k = 0; k < vectors.Count; k++)
        {
            for (int i = 1; i <= range; i++)
            {
                x = selectionPosition.X + (i * vectors[k].Item1);
                y = selectionPosition.Y + (i * vectors[k].Item2);

                if (!GameSystem.Map.IsInBounds(x, y)) break;
                if (GameSystem.Map.IsPassable(x, y)) continue;

                foreach (Position position in enemyPositions)
                {
                    if (position.X == x && position.Y == y)
                    {
                        enemyUnitCollisions.Add(position.Parent);
                        break;
                    }
                }

                break;
            }
        }
        return enemyUnitCollisions;          
    }

    void CheckEnableKingMovement()
    {
        foreach (Movement movement in movementComponentList)
        {
            Name name = GameSystem.EntityManager.GetComponent<Name>(movement.Parent);
            if (name.name == "King" && movement.Disabled) movement.Disabled = false;
        }            
    }
}