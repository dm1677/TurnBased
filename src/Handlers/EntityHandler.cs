using static GameSystem;

public class EntityHandler : IHandler
{
    public bool Process(Action action)
    {
        //if (GameSystem.Game.Turn.CheckTurnCount()) GameSystem.Game.Turn.ExecuteLastAction();
        ProcessEntityDeletion();
        GameSystem.Map.UpdatePassability(GameSystem.EntityManager.GetPositions());
        return true;
    }

    public void Reverse()
    {
        foreach (Entity entity in GameSystem.EntityManager.GetInactiveEntityList().Keys)
        {
            if (!entity.QueuedForDeletion)
            {
                GameSystem.EntityManager.RestoreEntity(entity.ID);
                break;
            }
        }
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