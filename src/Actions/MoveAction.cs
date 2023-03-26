using Godot;
using System.Collections.Generic;
using static GameSystem;


public class MoveAction : Action
{
    public int DestinationX { get; private set; }
    public int DestinationY { get; private set; }
    public int EntityID { get; private set; }
    public int FromX { get; private set; }
    public int FromY { get; private set; }
    
    private Position entityPosition;

    public MoveAction(int entityID, int destinationX, int destinationY)
    {
        EntityID = entityID;
        DestinationX = destinationX;
        DestinationY = destinationY;
    }

    public override void Execute()
    {
        entityPosition = GameSystem.EntityManager.GetComponent<Position>(EntityID);
        Movement entityMovement = GameSystem.EntityManager.GetComponent<Movement>(EntityID);

        if (entityMovement != null)
        {
            FromX = entityPosition.X;
            FromY = entityPosition.Y;

            entityPosition.X = DestinationX;
            entityPosition.Y = DestinationY;
        }
    }

    public override void Undo()
    {
        entityPosition.X = FromX;
        entityPosition.Y = FromY;
    }

    public override string[] ReturnData()
    {
        string[] data = new string[4];

        data[0] = GetType().ToString();
        data[1] = EntityID.ToString();
        data[2] = DestinationX.ToString();
        data[3] = DestinationY.ToString();

        return data;
    }
}
