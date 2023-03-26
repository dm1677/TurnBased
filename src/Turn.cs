using System;
using System.Collections.Generic;
using Godot;

public class Turn
{
	TurnState turnState;
	readonly GameActionManager actionManager;
	readonly GameContextManager contextManager;
	readonly HandlerManager handlerManager;
    private int turnCount = 1;

	public Turn(GameActionManager gameActionManager, GameContextManager contextManager, HandlerManager handlerManager)
	{
		this.contextManager = contextManager;
		this.handlerManager = handlerManager;
		SetTurnState((TurnState)GameSystem.Player.ID);
		actionManager = gameActionManager;
		if (contextManager.IsReplay)
			actionManager.LoadReplay(contextManager.GameInfo.ReplayPath);
	}

	public void AdvanceTurnState()
	{
		switch(turnState)
		{
			case TurnState.WaitForInput:
                SetTurnState(TurnState.ProcessMyTurn);
				break;
			case TurnState.ProcessMyTurn:
                SetTurnState(TurnState.WaitForEnemyInput);
                break;
			case TurnState.WaitForEnemyInput:
                SetTurnState(TurnState.ProcessEnemyTurn);
				break;
			case TurnState.ProcessEnemyTurn:
                SetTurnState(TurnState.WaitForInput);
                break;
		}
        GameSystem.Map.UpdatePassability(GameSystem.EntityManager.GetPositions());
    }

	public void ReverseTurnState()
	{
		switch (turnState)
		{
			case TurnState.WaitForInput:
				//SetTurnState(TurnState.ProcessEnemyTurn);
				break;
			case TurnState.ProcessMyTurn:
				SetTurnState(TurnState.WaitForInput);
				break;
			case TurnState.WaitForEnemyInput:
				//SetTurnState(TurnState.ProcessMyTurn);
				break;
			case TurnState.ProcessEnemyTurn:
				SetTurnState(TurnState.WaitForEnemyInput);
				break;
		}
        GameSystem.Map.UpdatePassability(GameSystem.EntityManager.GetPositions());
    }

    public void SwapTurn()
	{
        if (turnState == TurnState.WaitForInput)
            turnState = TurnState.WaitForEnemyInput;
        if (turnState == TurnState.WaitForEnemyInput)
            turnState = TurnState.WaitForInput;
    }

	public int GetTurnCount()
	{
		return turnCount;
	}

	public void IncrementTurnCount()
	{
		turnCount++;
	}

    //only for use with replays!
    public void DecrementTurnCount()
    {
        turnCount--;
    }

	void SetTurnState(TurnState state)
	{
		turnState = state;
	}

	public bool IsMyTurn()
	{
		if (turnState == TurnState.WaitForInput) return true;
		else return false;
	}

	public TurnState GetTurnState()
	{
		return turnState;
	}

	//Is the the best way of doing this?
	public void ExecuteLastAction()
	{
		actionManager.ExecuteLastAction();
		turnCount++;
	}

	public bool CheckTurnCount()
	{
		return (turnCount == actionManager.GetActionCount());
	}

	public void SetReplay()
	{
		actionManager.SaveReplay();
		actionManager.SetReplay(); //better way to do this?
	}

	public Action GetUpdatedAction()
	{
		return actionManager.GetUpdatedAction(); //Is there a better way of doing this?
	}

	public void TakeTurn(Action action)
	{
        actionManager.AddActionToList(action);
		AdvanceTurnState();
	}

	public bool ProcessTurn()
	{
		if (actionManager.GetActionCount() < 1) return false;
		Action action = actionManager.GetLastAction();
        if (handlerManager.IsValidAction(action))
		{
			ExecuteLastAction();
			handlerManager.ProcessHandlers(action);
			AdvanceTurnState();
            if (!contextManager.IsReplay) GameSystem.Sound.PlaySound(Sound.Effect.Turn);
            return true;
		}
		else
		{
			actionManager.RemoveInvalidAction(action);
			ReverseTurnState();
			return false;
		}
	}

	public bool CheckEntityOwnedByActivePlayer(Entity entity)
	{
        Owner o = GameSystem.EntityManager.GetComponent<Owner>(entity);
        User owner = o.ownedBy;
		User player = (User)GameSystem.Player.ID;
        User enemy = (User)GameSystem.Player.GetEnemyID();

		if (owner == player && turnState == TurnState.ProcessMyTurn
		 || owner == enemy && turnState == TurnState.ProcessEnemyTurn)
			return true;

		return false;
	}

    public bool MovingPlayerOwnsEntity(Entity entity)
    {
        Owner o = GameSystem.EntityManager.GetComponent<Owner>(entity);
        User owner = o.ownedBy;
        User player = (User)GameSystem.Player.ID;
        User enemy = (User)GameSystem.Player.GetEnemyID();

        if (owner == player && turnState == TurnState.WaitForInput
         || owner == enemy && turnState == TurnState.WaitForEnemyInput)
            return true;

        return false;
    }

	public void TurnStateMachine()
	{
		switch (GetTurnState())
		{
			case TurnState.ProcessMyTurn:

				if (!ProcessTurn()) break;

                var actionData = actionManager.GetLastAction().ReturnData();
				if (!contextManager.IsReplay && !contextManager.GameInfo.Singleplayer)
					GameSystem.Game.Rpc("RemoteAction", (object)actionData);

                break;
            case TurnState.ProcessEnemyTurn:

				if (!ProcessTurn()) break;

                break;
		}
	}

	//Should these two methods exist here?
	public void AdvanceReplay()
	{
        if (actionManager.GetReplayCount() == actionManager.GetActionCount()) return;

        var replayAction = actionManager.GetReplayAction(GetTurnCount() - 1);
		TakeTurn(replayAction);
    }

	public void ReverseReplay()
	{
        if (actionManager.GetActionCount() > 0)
        {
			Logging.Log("Reverse Replay: " + actionManager.GetLastAction().GetHashCode());
            var lastAction = actionManager.GetLastAction();
            actionManager.RemoveInvalidAction(lastAction);
            lastAction.Undo();

            AdvanceTurnState();
            handlerManager.ReverseHandlers();
			AdvanceTurnState();
			DecrementTurnCount();
        }
    }
}
