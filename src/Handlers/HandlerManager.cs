﻿using System.Collections.Generic;

public class HandlerManager
{
    private readonly List<IHandler> handlerList = new List<IHandler>();

    public HandlerManager(GameActionManager actionManager)
    {
        handlerList.Add(new MovementHandler(actionManager));
        handlerList.Add(new HealthHandler());
        handlerList.Add(new ResourceHandler(actionManager));
        handlerList.Add(new EntityHandler());
        handlerList.Add(new TimerHandler());
    }

    public bool ProcessHandlers()
    {
        foreach (IHandler handler in handlerList)
        {
            if (!handler.Process())
                return false;
        }

        System.GC.Collect(); //Godot memory leak - objects created when the mouse is moved are never disposed
        return true;
    }

    public void ReverseHandlers()
    {
        var reversedList = new List<IHandler>();
        reversedList.AddRange(handlerList);
        reversedList.Reverse();

        GameSystem.Game.Turn.AdvanceTurnState();

        foreach (IHandler handler in reversedList)
            handler.Reverse();

        GameSystem.Game.Turn.AdvanceTurnState();
        GameSystem.Game.Turn.DecrementTurnCount();
        GameSystem.Map.UpdatePassability(GameSystem.EntityManager.GetPositions());
        
        System.GC.Collect();
    }
}