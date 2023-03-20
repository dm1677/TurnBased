using static GameSystem;

public class SwapAction : Action
{
    public int SwappingEntity { get; private set; }
    public int SwappedEntity { get; private set; }

    public SwapAction(int swappingEntity, int swappedEntity)
    {
        SwappingEntity = swappingEntity;
        SwappedEntity = swappedEntity;
    }

    public override void Execute()
    {
        Position swappingPosition = GameSystem.EntityManager.GetComponent<Position>(SwappingEntity);
        Position swappedPosition = GameSystem.EntityManager.GetComponent<Position>(SwappedEntity);

        if(swappingPosition != null && swappedPosition != null)
        {
            int tempX, tempY;
            tempX = swappingPosition.X;
            tempY = swappingPosition.Y;

            swappingPosition.X = swappedPosition.X;
            swappingPosition.Y = swappedPosition.Y;

            swappedPosition.X = tempX;
            swappedPosition.Y = tempY;
        }
    }

    public override void Undo()
    {
        Position swappingPosition = GameSystem.EntityManager.GetComponent<Position>(SwappingEntity);
        Position swappedPosition = GameSystem.EntityManager.GetComponent<Position>(SwappedEntity);

        if (swappingPosition != null && swappedPosition != null)
        {
            int tempX, tempY;
            tempX = swappingPosition.X;
            tempY = swappingPosition.Y;

            swappingPosition.X = swappedPosition.X;
            swappingPosition.Y = swappedPosition.Y;

            swappedPosition.X = tempX;
            swappedPosition.Y = tempY;
        }
    }

    public override string[] ReturnData()
    {
        string[] data = new string[3];

        data[0] = GetType().ToString();
        data[1] = SwappingEntity.ToString();
        data[2] = SwappedEntity.ToString();

        return data;
    }
}