public class AIMoveAction : AIAction
{
    public int x, y;
    public UnitState unit;

    public AIMoveAction(int x, int y, UnitState unit)
    {
        this.x = x;
        this.y = y;
        this.unit = unit;
    }

    public static bool CheckMovement(MovementType movementType, GameState gameState, UnitState unit, int destinationX, int destinationY)
    {
        switch (movementType)
        {
            case MovementType.Line:
                return LineMovement(gameState, unit, destinationX, destinationY);

            case MovementType.Diagonal:
                return DiagonalMovement(gameState, unit, destinationX, destinationY);

            case MovementType.LineAndDiagonal:
                if (LineMovement(gameState, unit, destinationX, destinationY) || DiagonalMovement(gameState, unit, destinationX, destinationY))
                    return true;
                else return false;
        }

        return false;
    }

    static bool LineMovement(GameState gameState, UnitState unit, int destinationX, int destinationY)
    {
        int i;
        int speed = unit.UnitClass.Speed;
        Coords currentPosition = new Coords(unit.X, unit.Y);
        Coords destination = new Coords(destinationX, destinationY);

        for (int x = 0; x <= speed; x++)
        {
            i = currentPosition.X + x;

            //Make sure we're not checking a tile outside of the bounds of the map array
            if (i > gameState.mapWidth - 1) break;

            //If we're checking an impassable tile (not including the tile our current selection is standing on), don't allow this loop to continue
            //-this way we can't jump over impassable tiles
            if (i != currentPosition.X && !gameState.Passable(i, currentPosition.Y)) goto A;

            if (destination.X == i && destination.Y == currentPosition.Y) return true;
        }
    A:
        for (int x = 0; x >= -speed; x--)
        {
            i = currentPosition.X + x;

            if (i < 0) break;

            if (i != currentPosition.X && !gameState.Passable(i, currentPosition.Y)) goto B;

            if (destination.X == i && destination.Y == currentPosition.Y) return true;
        }


    B:
        for (int y = 0; y <= speed; y++)
        {
            i = currentPosition.Y + y;

            if (i > gameState.mapHeight - 1) break;

            if (i != currentPosition.Y && !gameState.Passable(currentPosition.X, i)) goto C;

            if (destination.Y == i && destination.X == currentPosition.X) return true;
        }

    C:
        for (int y = 0; y >= -speed; y--)
        {
            i = currentPosition.Y + y;

            if (i < 0) break;

            if (i != currentPosition.Y && !gameState.Passable(currentPosition.X, i)) break;

            if (destination.Y == i && destination.X == currentPosition.X) return true;
        }


        return false;
    }

    static bool DiagonalMovement(GameState gameState, UnitState unit, int destinationX, int destinationY)
    {
        int i, x, y;
        int width = gameState.mapWidth;
        int height = gameState.mapHeight;
        int speed = unit.UnitClass.Speed;
        Coords currentPosition = new Coords(unit.X, unit.Y);
        Coords destination = new Coords(destinationX, destinationY);

        for (i = 0; i <= speed; i++)
        {
            x = currentPosition.X + i;
            y = currentPosition.Y - i;

            //Make sure we're not checking a tile outside of the bounds of the map array
            if (!IsInMapLimits(x, y, width, height)) break;

            //If we're checking an impassable tile (not including the tile our current selection is standing on), don't allow this loop to continue
            //-this way we can't jump over impassable tiles
            if (x != currentPosition.X && y != currentPosition.Y && !gameState.Passable(x, y)) goto A;

            if (destination.X == x && destination.Y == y) return true;
        }
    A:
        for (i = 0; i <= speed; i++)
        {
            x = currentPosition.X + i;
            y = currentPosition.Y + i;

            if (!IsInMapLimits(x, y, width, height)) break;

            if (x != currentPosition.X && y != currentPosition.Y && !gameState.Passable(x, y)) goto B;

            if (destination.X == x && destination.Y == y) return true;
        }


    B:
        for (i = 0; i <= speed; i++)
        {
            x = currentPosition.X - i;
            y = currentPosition.Y + i;

            if (!IsInMapLimits(x, y, width, height)) break;

            if (x != currentPosition.X && y != currentPosition.Y && !gameState.Passable(x, y)) goto C;

            if (destination.X == x && destination.Y == y) return true;
        }

    C:
        for (i = 0; i <= speed; i++)
        {
            x = currentPosition.X - i;
            y = currentPosition.Y - i;

            if (!IsInMapLimits(x, y, width, height)) break;

            if (x != currentPosition.X && y != currentPosition.Y && !gameState.Passable(x, y)) break;

            if (destination.X == x && destination.Y == y) return true;
        }


        return false;
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