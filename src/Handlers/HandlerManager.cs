using Godot;
using System.Collections.Generic;

public class HandlerManager
{
    private readonly List<IHandler> handlerList = new List<IHandler>();

    public HandlerManager()
    {
        handlerList.Add(new MovementHandler());
        handlerList.Add(new HealthHandler());
        handlerList.Add(new ResourceHandler());
        handlerList.Add(new EntityHandler());
        handlerList.Add(new TimerHandler());
    }

    public bool IsValidAction(Action action)
    {
        foreach (IHandler handler in handlerList)
        {
            if (!handler.Validate(action))
                return false;
        }
        return true;
    }

    public bool ProcessHandlers(Action action)
    {
        GameSystem.Map.UpdatePassability(GameSystem.EntityManager.GetPositions());
        foreach (IHandler handler in handlerList)
        {
            if (!handler.Process(action))
                return false;
        }
        GameSystem.Map.UpdatePassability(GameSystem.EntityManager.GetPositions());
        System.GC.Collect(); //Godot memory leak - objects created when the mouse is moved are never disposed
        return true;
    }

    public void ReverseHandlers()
    {
        for (int i = handlerList.Count - 1; i >= 0; i--)
            handlerList[i].Reverse();

        GameSystem.Map.UpdatePassability(GameSystem.EntityManager.GetPositions());
        
        System.GC.Collect();
    }
}