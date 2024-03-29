using static GameSystem;

public class EntityHandler : IHandler
{
    public bool Validate(Action action)
    {
        return true;
    }

    public bool Process(Action action)
    {
        ProcessEntityDeletion();
        GameSystem.Map.UpdatePassability(GameSystem.EntityManager.GetPositions());
        return true;
    }

    public void Reverse()
    {
    }

    void ProcessEntityDeletion()
    {
        foreach (Entity entity in GameSystem.EntityManager.GetEntityList().Keys)
        {
            if (entity.QueuedForDeletion)
            {
                if (Input.GetSelection() == entity)
                    Input.SetNullSelection();
                GameSystem.EntityManager.DeleteEntity(entity);
                break;
            }
        }
    }
}