using Godot;
using System;
using System.Collections.Generic;
using static GameSystem;

public class Game : Node2D
{
    const int _tileSize = 32;
    const int _mapWidth = 15;
    const int _mapHeight = 15;

    private const string UIScenePath = "res://src/UI/GameUI.tscn";
    private const string RenderScenePath = "res://src/Scenes/Render.tscn";

    //Godot: Parent nodes with networking information
    GameSetup gameSetup;
    public GameUI UI { get; private set; }
    Render render;

    AIManager ai = new AIManager();

    public Sync Sync { get; private set; }
    public GameContextManager ContextManager { get; private set; }
    public Turn Turn { get; private set; }

    public bool IsReplay { get => ContextManager.IsReplay; }
    public bool IsSingleplayer { get => ContextManager.GameInfo.Singleplayer; }
    
    public bool TimerDataReceived { get; set; } = false;

    private readonly Component[] kingMovementComponents = new Component[4];

    public void Initialise(GameInfo gameInfo, HashSet<PlayerInfo> playerInfoList)
    {
        ContextManager = new GameContextManager(gameInfo);
        InitialiseGameSystem(this, _mapWidth, _mapHeight, playerInfoList);
        var actionManager = new GameActionManager();
        Turn = new Turn(actionManager, ContextManager, new HandlerManager(actionManager));

        gameSetup = (GameSetup)GetParent();
        Sync = gameSetup.sync;

        CreateStartingEntities();

        render = (Render)InstantiateChildNode(GD.Load(RenderScenePath));
        UI = (GameUI)InstantiateChildNode(GD.Load(UIScenePath));

        if (gameInfo.GameType == GameType.Replay)
            UI.DisableSurrender();
        UI.Initialise();
        UI.ChatMessage(GameSystem.Player.Name + " joined the game!");
        GameSystem.Map.UpdatePassability(GameSystem.EntityManager.GetPositions());
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

        foreach (UnitState u in gameState.units)
            ComponentFactory.Instance().CreateUnit(u.X, u.Y, u.UnitType, u.Owner);

        CreateResources();
        CreateKings();
    }

    //get rid of the string matching!
    public GameState EncodeGameState()
    {
        User toPlay = User.Enemy;
        if (Turn.IsMyTurn()) toPlay = User.Player;

        GameState state = new GameState(_mapWidth, _mapHeight, toPlay);
        var list = GameSystem.EntityManager.GetEntityList();

        foreach (Entity entity in list.Keys)
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

            if (name != null)
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

    void CreateResources()
    {
        GameSystem.Player.ResourceEntity = ComponentFactory.Instance().CreateResource(GameSystem.Player.ID);
        GameSystem.Player.Resource = GameSystem.EntityManager.GetComponent<GResource>(GameSystem.Player.ResourceEntity);

        Enemy.ResourceEntity = ComponentFactory.Instance().CreateResource(Enemy.ID);
        Enemy.Resource = GameSystem.EntityManager.GetComponent<GResource>(Enemy.ResourceEntity);
    }

    void CreateKings()
    {
        Entity[] kings =
        {
            ComponentFactory.Instance().CreateUnit(0, 0, Unit.King, User.Player),
            ComponentFactory.Instance().CreateUnit(_mapWidth - 1, _mapHeight - 1, Unit.King, User.Enemy),
            ComponentFactory.Instance().CreateUnit(_mapWidth - 1, 0, Unit.King, User.Enemy),
            ComponentFactory.Instance().CreateUnit(0, _mapHeight - 1, Unit.King, User.Player)
        };

        for (int i = 0; i < kings.Length; i++)
        {
            kingMovementComponents[i] = GameSystem.EntityManager.GetComponent<Movement>(kings[i]);
            kingMovementComponents[i].Disabled = true;
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

        Turn.TakeTurn((Action)action);

        OS.RequestAttention();
    }

    [Remote]
    void ValidateTimer(Godot.Collections.Array data)
    {
        Enemy.ServerCurrentTime = (float)data[0];
        TimerDataReceived = true;
    }

    public override void _Process(float delta)
    {
        CheckSetKingMovement();

        if (!IsReplay && !ContextManager.Context.GameOver && !ContextManager.GameInfo.Singleplayer)
        {
            GameSystem.Player.ProcessTimer();
            Enemy.ProcessTimer();
        }


        if (!ContextManager.Context.GameOver && ContextManager.GameInfo.Singleplayer && Turn.GetTurnState() == TurnState.WaitForEnemyInput)
        {
            var aiAction = ai.GetAction(EncodeGameState(), (User)GameSystem.Player.GetEnemyID());
            var action = GameSystem.Input.ConvertAction(aiAction, (User)GameSystem.Player.GetEnemyID());

            if (action != null)
                Turn.TakeTurn(action);
            else ContextManager.SetGameOver();
        }

        Turn.TurnStateMachine();

        Action lastAction = Turn.GetUpdatedAction();
        if (lastAction != null)
            UI.ChatActionLog(lastAction); //Should/can the null check be deferred?

        if (ContextManager.Context.GameOver) CheckRematch();

        Update();
    }

    private void CheckRematch()
    {
        if (ContextManager.Context.LocalRematch && ContextManager.Context.RemoteRematch && GetTree().HasNetworkPeer() && GetTree().GetNetworkUniqueId() == 1)
        {
            Rpc("DisposeNode");
            gameSetup.Rpc("StartGame", (object)ContextManager.GameInfo.Serialise());
        }
    }

    [RemoteSync]
    private void DisposeNode()
    {
        render.SetProcess(false);
        var a = render.GetChildren();
        foreach (var child in a)
        {
            if (child is Node m)
                m.Free();
        }
        QueueFree();
    }

    private void CheckSetKingMovement()
    {
        if (Turn.GetTurnCount() == 2)
        {
            foreach (Component component in kingMovementComponents)
                component.Disabled = false;
        }
    }

    public void Rematch()
    {
        ContextManager.SetLocalRematch();
        Rpc("SetRematch");
    }

    [Remote]
    public void SetRematch()
    {
        ContextManager.SetRemoteRematch();
    }

    public void SetNullSelection() { GameSystem.Input.SetNullSelection(); }

    [RemoteSync]
    public void GameResult(string winner)
    {
        UI.LocalChatMessage($"{winner} has won the game!");
        UI.DisableSurrender();
        GameSystem.Input.processActionInput = false;
        ContextManager.SetGameOver();

        if (!IsReplay)
        {
            Turn.SetReplay();
            ContextManager.SetGameTypeToReplay();
        }
    }

    //Getters
    public int GetTileSize() { return _tileSize; }
}
