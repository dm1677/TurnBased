using Godot;
using System;
using System.Collections.Generic;
using static GameSystem;

public class Game : Node2D
{
	const int _tileSize = 32;
	const int _mapWidth = 15;
	const int _mapHeight = 15;

	//Godot: Parent nodes with networking information
	GameSetup gameSetup;
	Lobby lobby;
	public GameUI ui { get; private set; }
	Render render;

	AIManager ai = new AIManager();

	public Sync sync;

	public GameInfo gameInfo;

	public HashSet<PlayerInfo> playerInfo;

	public bool isReplay { get; set; }
	public bool gameOver = false;
	public bool timerDataReceived = false;
	public bool rematch = false;
	public bool opponentRematch = false;

	private Component[] kingMovementComponents = new Component[4];
	private Action lastAction;

	public void Initialise(GameInfo gameInfo, HashSet<PlayerInfo> playerInfoList)
	{
		this.gameInfo = gameInfo;

		playerInfo = playerInfoList;
		Instantiate();

		isReplay = gameInfo.Replay;
		if (isReplay)
		{
			GameSystem.Turn.DeserialiseList(gameInfo.ReplayPath);
			ui.DisableSurrender();
		}

		ui.Initialise();
		ui.ChatMessage(GameSystem.Player.GetName() + " joined the game!");
		GameSystem.Map.UpdatePassability(GameSystem.EntityManager.GetPositions());
	}
	
	//Must call this first for everything to work!
	void Instantiate()
	{
		gameSetup = (GameSetup)GetParent();
		sync = gameSetup.sync;

		InitialiseGameSystem(this, _mapWidth, _mapHeight, playerInfo);
		CreateStartingEntities();

		render = (Render)InstantiateChildNode(GD.Load("res://src/Scenes/Render.tscn"));
		ui = (GameUI)InstantiateChildNode(GD.Load("res://src/UI/GameUI.tscn"));
	}

	//Temporary
	void CreateStartingEntities()
	{
		var gameState = new GameState(15, 15, User.Player); //3rd argument probably doesn't matter at the moment
		
		gameState.AddUnit(new UnitState(Unit.Tree, User.Neutral, 0, 6, 6));
		gameState.AddUnit(new UnitState(Unit.Tree, User.Neutral, 0, 8, 8));
		gameState.AddUnit(new UnitState(Unit.Tree, User.Neutral, 0, 6, 8));
		gameState.AddUnit(new UnitState(Unit.Tree, User.Neutral, 0, 8, 6));
		gameState.AddUnit(new UnitState(Unit.Tree, User.Neutral, 0, 1, 7));
		gameState.AddUnit(new UnitState(Unit.Tree, User.Neutral, 0, 13, 7));
		gameState.AddUnit(new UnitState(Unit.Tree, User.Neutral, 0, 7, 13));
		gameState.AddUnit(new UnitState(Unit.Tree, User.Neutral, 0, 7, 1));

		/*gameState.AddUnit(new UnitState(ComponentFactory.Unit.Gobbo, global::Owner.Player.Player, 3, 3, 3));
		gameState.AddUnit(new UnitState(ComponentFactory.Unit.Gobbo, global::Owner.Player.Player, 3, 3, 2));
		gameState.AddUnit(new UnitState(ComponentFactory.Unit.Gobbo, global::Owner.Player.Player, 3, 3, 11));
		gameState.AddUnit(new UnitState(ComponentFactory.Unit.Gobbo, global::Owner.Player.Player, 3, 3, 12));

		gameState.AddUnit(new UnitState(ComponentFactory.Unit.Prawn, global::Owner.Player.Player, 0, 4, 5));
		gameState.AddUnit(new UnitState(ComponentFactory.Unit.Prawn, global::Owner.Player.Player, 0, 4, 9));

		gameState.AddUnit(new UnitState(ComponentFactory.Unit.Knight, global::Owner.Player.Player, 0, 2, 7));


		gameState.AddUnit(new UnitState(ComponentFactory.Unit.Gobbo, global::Owner.Player.Enemy, 3, 11, 3));
		gameState.AddUnit(new UnitState(ComponentFactory.Unit.Gobbo, global::Owner.Player.Enemy, 3, 11, 2));
		gameState.AddUnit(new UnitState(ComponentFactory.Unit.Gobbo, global::Owner.Player.Enemy, 3, 11, 11));
		gameState.AddUnit(new UnitState(ComponentFactory.Unit.Gobbo, global::Owner.Player.Enemy, 3, 11, 12));

		gameState.AddUnit(new UnitState(ComponentFactory.Unit.Prawn, global::Owner.Player.Enemy, 0, 10, 5));
		gameState.AddUnit(new UnitState(ComponentFactory.Unit.Prawn, global::Owner.Player.Enemy, 0, 10, 9));

		gameState.AddUnit(new UnitState(ComponentFactory.Unit.Knight, global::Owner.Player.Enemy, 0, 12, 7));*/
		

		foreach (UnitState u in gameState.units)
		{
			var entity = ComponentFactory.Instance().CreateUnit(u.X, u.Y, u.UnitType);
			GameSystem.EntityManager.AddComponent(entity, new Owner() { ownedBy = u.Owner });
		}

		//CreateTrees();
		CreateResources();
		CreateKings();

		if (!isReplay)
			CreateTimers();

		//var ai = new TreeSearchNode(EncodeGameState());
		//var b = ai.BestAction();
		//GD.Print(b);
		//Debug(ai);

	}

	//get rid of the string matching!
	public GameState EncodeGameState()
	{
		User toPlay = User.Enemy;
		if (GameSystem.Turn.IsMyTurn()) toPlay = User.Player;

		GameState state = new GameState(_mapWidth, _mapHeight, toPlay);
		var list = GameSystem.EntityManager.GetEntityList();

		foreach(Entity entity in list.Keys)
		{
			Name name = GameSystem.EntityManager.GetComponent<Name>(entity);
			Owner owner = GameSystem.EntityManager.GetComponent<Owner>(entity);
			Health health = GameSystem.EntityManager.GetComponent<Health>(entity);
			Position position = GameSystem.EntityManager.GetComponent<Position>(entity);
			
			var player = owner.ownedBy;
			int hp = 0, x = 0, y = 0;

			if (health != null) hp = health.CurrentHP;
			if (position != null) { x = position.X; y = position.Y; }

			Unit unitType = Unit.Tree;

			if(name != null)
			{
				switch (name.name)
				{
					case "Prawn":
						unitType = Unit.Prawn;
						break;
					case "King":
						unitType = Unit.King;
						break;
					case "Knight":
						unitType = Unit.Knight;
						break;
					case "Gobbo":
						unitType = Unit.Gobbo;
						break;
					case "Statue":
						unitType = Unit.Building;
						break;
					case "Money":
						GResource resource = GameSystem.EntityManager.GetComponent<GResource>(entity);
						hp = resource.Value;
						x = -1;
						y = -1;
						unitType = Unit.Resource;
						break;
					case "Tree":
						unitType = Unit.Tree;
						break;
				}
				state.AddUnit(new UnitState(unitType, player, hp, x, y));
			}            
		}

		return state;
	}

	void CreateTrees()
	{
		ComponentFactory.Instance().CreateTree(6, 6);
		ComponentFactory.Instance().CreateTree(8, 8);
		ComponentFactory.Instance().CreateTree(6, 8);
		ComponentFactory.Instance().CreateTree(8, 6);
		ComponentFactory.Instance().CreateTree(1, 7);
		ComponentFactory.Instance().CreateTree(13, 7);
		ComponentFactory.Instance().CreateTree(7, 1);
		ComponentFactory.Instance().CreateTree(7, 13);
	}
	
	void CreateTimers()
	{
		var timerEntity = ComponentFactory.Instance().CreateTimer(gameInfo.TimerType, gameInfo.Time, gameInfo.Increment, GameSystem.Player.GetID());
		GameSystem.Player.SetTimer(timerEntity);

		timerEntity = Enemy.TimerEntity = ComponentFactory.Instance().CreateTimer(gameInfo.TimerType, gameInfo.Time, gameInfo.Increment, Enemy.GetID());
		Enemy.SetTimer(timerEntity);
	}

	void CreateResources()
	{
		GameSystem.Player.ResourceEntity = ComponentFactory.Instance().CreateResource(GameSystem.Player.GetID());
		GameSystem.Player.Resource = GameSystem.EntityManager.GetComponent<GResource>(GameSystem.Player.ResourceEntity);

		Enemy.ResourceEntity = ComponentFactory.Instance().CreateResource(Enemy.GetID());
		Enemy.Resource = GameSystem.EntityManager.GetComponent<GResource>(Enemy.ResourceEntity);
	}

	//Temporary
	void CreateKings()
	{
		var king = ComponentFactory.Instance().CreateUnit(0, 0, Unit.King);
		GameSystem.EntityManager.AddComponent(king, new Owner() { ownedBy = User.Player });
		kingMovementComponents[0] = GameSystem.EntityManager.GetComponent<Movement>(king);
		kingMovementComponents[0].Disabled = true;

		king = ComponentFactory.Instance().CreateUnit(_mapWidth - 1, _mapHeight - 1, Unit.King);
		GameSystem.EntityManager.AddComponent(king, new Owner() { ownedBy = User.Enemy });
		kingMovementComponents[1] = GameSystem.EntityManager.GetComponent<Movement>(king);
		kingMovementComponents[1].Disabled = true;

		//if (gameInfo.KingCount == 1) //fix replay king count
		{
			king = ComponentFactory.Instance().CreateUnit(_mapWidth - 1, 0, Unit.King);
			GameSystem.EntityManager.AddComponent(king, new Owner() { ownedBy = User.Enemy });
			kingMovementComponents[2] = GameSystem.EntityManager.GetComponent<Movement>(king);
			kingMovementComponents[2].Disabled = true;

			king = ComponentFactory.Instance().CreateUnit(0, _mapHeight - 1, Unit.King);
			GameSystem.EntityManager.AddComponent(king, new Owner() { ownedBy = User.Player });
			kingMovementComponents[3] = GameSystem.EntityManager.GetComponent<Movement>(king);
			kingMovementComponents[3].Disabled = true;
		}
	}

	public void EndGame()
	{
		gameSetup.EndSession();
		QueueFree(); //Godot: deletes the node
	}

	//Chatbox in the UI sets this so you can't use the keyboard for game actions while you're typing
	//Probably some better way provided by godot somewhere
	public void ChatInput(bool b)
	{
		GameSystem.Input.AcceptKeyboardInput(b);
	}

	//godot helper function, not sure where to put this
	public static Node InstantiateChildNode(Resource scene, Node node)
	{
		var gameNode = (PackedScene)scene;
		var nodeInstance = gameNode.Instance();

		node.AddChild(nodeInstance);
		return nodeInstance;
	}

	Node InstantiateChildNode(Resource scene)
	{
		var gameNode = (PackedScene)scene;
		var nodeInstance = gameNode.Instance();

		AddChild(nodeInstance);
		return nodeInstance;
	}

	//Input processing offloaded to InputHelper
	public override void _Input(InputEvent inputEvent)
	{
		GameSystem.Input.ProcessInput(inputEvent);
	}

	//Takes data from an RPC
	//This data is used to create an Action
	[Remote]
	void RemoteAction(string[] data)
	{
		var type = Type.GetType(data[0]);
		object[] parameters = new object[data.Length - 1];

		for (int i = 1; i < data.Length; i++)
			parameters[i - 1] = data[i].ToInt();

		var action = Activator.CreateInstance(type, parameters);

		GameSystem.Turn.TakeTurn((Action)action);

		OS.RequestAttention();
	}

	[Remote]
	void ValidateTimer(Godot.Collections.Array data)
	{
		Enemy.ServerCurrentTime = (float)data[0];
		timerDataReceived = true;
	}

	public override void _Process(float delta)
	{
		CheckSetKingMovement();
		
		if (!isReplay && !gameOver && !gameInfo.Singleplayer)
		{
			GameSystem.Player.ProcessTimer();
			Enemy.ProcessTimer();
		}


		if (!gameOver && gameInfo.Singleplayer && GameSystem.Turn.GetTurnState() == TurnState.WaitForEnemyInput)
		{
			var aiAction = ai.GetAction(EncodeGameState(), (User)GameSystem.Player.GetEnemyID());
			var action = GameSystem.Input.ConvertAction(aiAction, (User)GameSystem.Player.GetEnemyID());

			if (action != null)
				GameSystem.Turn.TakeTurn(action);
			else gameOver = true;
		}

		GameSystem.Turn.TurnStateMachine();


		if ((GameSystem.Turn.GetListSize() > 0 && lastAction != GameSystem.Turn.GetLastAction()) || (lastAction == null && GameSystem.Turn.GetListSize() >= 1))
		{
			ui.ChatActionLog(GameSystem.Turn.GetLastAction());
			lastAction = GameSystem.Turn.GetLastAction();
		}

		if (gameOver) CheckRematch();

		Update();
	}

	private void CheckRematch()
	{
		if (rematch && opponentRematch && GetTree().HasNetworkPeer() && GetTree().GetNetworkUniqueId() == 1)
		{
			Rpc("DisposeNode");
			gameSetup.Rpc("StartGame", (object)gameInfo.Serialise());			
		}
	}

	[RemoteSync]
	private void DisposeNode()
	{
		render.SetProcess(false);
		var a = render.GetChildren();
		foreach(var child in a)
		{
			if (child is Node m)
				m.Free();
		}
		QueueFree();
	}

	private void CheckSetKingMovement()
	{
		if (GameSystem.Turn.GetTurnCount() == 2)
		{
			//for (int i = 0; i < gameInfo.KingCount * 2; i++) //fix here
			{
				kingMovementComponents[0].Disabled = false;
				kingMovementComponents[1].Disabled = false;
				kingMovementComponents[2].Disabled = false;
				kingMovementComponents[3].Disabled = false;
			}
		}
	}

	public void Rematch()
	{
		rematch = true;
		Rpc("SetRematch");
	}

	[Remote]
	public void SetRematch()
	{
		opponentRematch = true;
	}

	public void SetNullSelection() { GameSystem.Input.SetNullSelection(); }

	[RemoteSync]
	public void GameResult(string winner)
	{
		ui.LocalChatMessage($"{winner} has won the game!");
		ui.DisableSurrender();
		GameSystem.Input.processActionInput = false;
		gameOver = true;

		if (!isReplay)
		{
			GameSystem.Turn.SerialiseList();
			GameSystem.Turn.SetReplay();
		}
	}

	//Getters
	public int GetTileSize() { return _tileSize; }
}
