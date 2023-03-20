using System;
using System.Collections.Generic;
using Godot;

public class Turn
{
	TurnState turnState;

	private readonly List<Action> actionList = new List<Action>();
    private readonly List<Action> replayList = new List<Action>();

    private int turnCount = 1;

	//Always define whether or not it's the player's turn when instantiating
	public Turn()
	{
		SetTurnState((TurnState)GameSystem.Player.GetID());
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


	public void AddActionToList(Action action)
	{
		actionList.Add(action);
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

	public void ExecuteLastAction()
	{
        if (actionList.Count > 0)
        {
            GetLastAction().Execute();
            turnCount++;
        }
	}

	public bool CheckTurnCount()
	{
		if (turnCount == actionList.Count) return true;
		return false;
	}


	public Action GetLastAction()
	{
        if (actionList.Count > 0)
		{
			return actionList[actionList.Count - 1];
		}
		else throw new Exception("There are no actions in the list.");
	}

	public void ExecuteAction()
	{
		if (actionList.Count > 0)
		{
			actionList[turnCount - 1].Execute();
			IncrementTurnCount();
		}
		else
			throw new Exception("There are no actions in the list.");
	}

	public int GetListSize()
	{
		return actionList.Count;
	}

	public void TakeTurn(Action action)
	{
		AddActionToList(action);
		AdvanceTurnState();
	}

	public bool ProcessTurn()
	{
		if (actionList.Count < 1) return false;
		if (GameSystem.HandlerManager.ProcessHandlers())
		{
			AdvanceTurnState();
            if (!GameSystem.Game.isReplay)GameSystem.Sound.PlaySound(Sound.Effect.Turn);
            return true;
		}
		else
		{
			ReverseTurnState();
			return false;
		}
	}

	public void RemoveInvalidAction(Action action)
	{
		actionList.Remove(action);
	}

	public void ReplaceLastAction(Action action)
	{
		actionList.RemoveAt(actionList.Count - 1);
		actionList.Add(action);
	}

	public bool CheckEntityOwnedByActivePlayer(Entity entity)
	{
        Owner o = GameSystem.EntityManager.GetComponent<Owner>(entity);
        User owner = o.ownedBy;
		User player = (User)GameSystem.Player.GetID();
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
        User player = (User)GameSystem.Player.GetID();
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

                var actionData = GetLastAction().ReturnData();
				if (!GameSystem.Game.isReplay && !GameSystem.Game.gameInfo.Singleplayer)
                    GameSystem.Game.Rpc("RemoteAction", (object)actionData);

                break;
            case TurnState.ProcessEnemyTurn:

				if (!ProcessTurn()) break;

                break;
		}
	}

    public void SerialiseList()
    {
        string dir = AppDomain.CurrentDomain.BaseDirectory;
        string replayDir = @"\Replays\LastReplay.tbr";
        string path = dir + replayDir;

        System.IO.Directory.CreateDirectory(dir + @"\Replays");
        
        try
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(path))
            {
                foreach (Action action in actionList)
                {
                    var data = action.ReturnData();
                    sw.WriteLine(";");
                    foreach (string s in data)
                        sw.WriteLine(s);
                }
                sw.WriteLine(";");
            }
        }
        catch(Exception e)
        {
            GD.Print("SerialiseList exception: " + e);
        }
    }

    public void DeserialiseList(string path)
    {
        var lines = System.IO.File.ReadLines(path);
        var actionData = new List<string>();
        
        foreach (string line in lines)
        {
            if (line == ";")
            {
				if (actionData.Count > 0)
				{
					var action = DeserialiseAction(actionData.ToArray());
					replayList.Add(action);

					actionData.Clear();
				}
            }
            else actionData.Add(line);
            
        }
    }

    Action DeserialiseAction(string[] data)
    {
        var type = Type.GetType(data[0]);
        object[] parameters = new object[data.Length - 1];

        for (int i = 1; i < data.Length; i++)
            parameters[i - 1] = int.Parse(data[i]);

        var action = Activator.CreateInstance(type, parameters);

        return (Action)action;
    }

    public void SetReplay()
    {
        replayList.AddRange(actionList);
        GameSystem.Game.isReplay = true;
    }

	public Action GetReplayAction(int index)
	{
		return replayList[index];
	}

	public int GetReplayCount()
	{
		return replayList.Count;
	}

}
