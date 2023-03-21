using System.Collections.Generic;
using static GameSystem;

public class MovementHandler : IHandler
{
    readonly HashSet<Movement> movementComponentList = new HashSet<Movement>();
    readonly GameActionManager actionManager;

    public MovementHandler(GameActionManager actionManager)
    {
        this.actionManager = actionManager;
    }

    public bool Process()
    {
        UpdateComponentList();

        CheckEnableKingMovement();

        var action = actionManager.GetLastAction();
        if (CheckMovementAction(action))
        {
            if (CheckValidAction(action) || GameSystem.Game.IsReplay)
            {
                actionManager.ExecuteLastAction();
                return true;
            }
            else
            {
                actionManager.RemoveInvalidAction(action);
                return false;
            }
        }
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

    bool CheckMovementAction(Action action)
    {
        if (action is MoveAction ||
            action is AttackAction ||
            action is SwapAction)
            return true;
        return false;
    }

    bool CheckValidAction(Action action)
    {
        if (action is MoveAction moveAction)
        {
            var destination = new Coords(moveAction.DestinationX, moveAction.DestinationY);

            var entity = GameSystem.EntityManager.GetEntity(moveAction.EntityID);


            Position position = GameSystem.EntityManager.GetComponent<Position>(entity);
            Movement movement = GameSystem.EntityManager.GetComponent<Movement>(entity);


            if (GameSystem.Game.Turn.CheckEntityOwnedByActivePlayer(entity))
            {
                var enemyUnit = CheckNearbyEnemies(entity, destination);
                if (enemyUnit != null)
                    return CheckValidAction(new AttackAction(entity.ID, enemyUnit.ID));
                return CheckMovement(destination, position, movement);
            }
            else return false;
        }
        else if (action is AttackAction AttackAction)
        {
            if (GameSystem.Game.IsReplay) return true;
            actionManager.ReplaceLastAction(action);
            return true;
        }
        else if (action is SwapAction swapAction)
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

        return true;
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

    public static List<Entity> Attack(Entity selection, AttackType direction)
    {
        List<Entity> enemyUnitCollisions = new List<Entity>();
        if (selection == null) return enemyUnitCollisions;

        var selectionList = GameSystem.EntityManager.GetComponentList(selection);

        Owner owner = GameSystem.EntityManager.GetComponent<Owner>(selection);
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

        foreach (Entity entity in GameSystem.EntityManager.GetEnemyUnits(ownedBy))
        {
            Position pos = GameSystem.EntityManager.GetComponent<Position>(entity);
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