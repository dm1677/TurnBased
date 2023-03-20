using System;
using Godot;
using static GameSystem;

public class InputHelper
{
    Entity selection;

    readonly int _tileSize, playerID;

    bool acceptKeyboardInput = true;
    bool acceptMouseInput = true;
    public bool processActionInput = true;

    AIManager ai = new AIManager();

    public InputHelper()
    {
        playerID = GameSystem.Player.GetID();
        _tileSize = GameSystem.Game.GetTileSize();
    }

    //TODO: Change this to check whether the mouse is within a specific viewport
    private bool MouseIsInBounds()
    {
        var mousePos = GameSystem.Game.GetGlobalMousePosition();

        var maxX = _tileSize * GameSystem.Map.Width;
        var maxY = _tileSize * GameSystem.Map.Height;

        if (mousePos.x > maxX || mousePos.y > maxY) return false;
        else return true;
    }

    //Gets the tile's array coordinates at the current mouse position
    public Coords GetTilePositionAtMouse()
    {
        var x = (int)(float)Math.Floor(GameSystem.Game.GetLocalMousePosition().x / _tileSize);
        var y = (int)(float)Math.Floor(GameSystem.Game.GetLocalMousePosition().y / _tileSize);

        if (x < 0) x = 0;
        if (y < 0) y = 0;

        if (x >= GameSystem.Map.Width) x = GameSystem.Map.Width - 1;
        if (y >= GameSystem.Map.Height) y = GameSystem.Map.Height - 1;

        return new Coords(x, y);

    }

    //Gets the mouse position and locks it to grid
    public Coords GetClampedMousePosition()
    {
        var x = (int)(float)Math.Floor(GameSystem.Game.GetLocalMousePosition().x / _tileSize);
        var y = (int)(float)Math.Floor(GameSystem.Game.GetLocalMousePosition().y / _tileSize);

        if (x < 0) x = 0;
        if (y < 0) y = 0;

        if (x > GameSystem.Map.Width) x = GameSystem.Map.Width;
        if (y > GameSystem.Map.Height) y = GameSystem.Map.Height;


        return new Coords(x * _tileSize, y * _tileSize);
    }

    public Entity GetSelection()
    {
        return selection;
    }

    public void SetNullSelection()
    {
        selection = null;
    }

    public void AcceptKeyboardInput(bool accept)
    {
        acceptKeyboardInput = accept;
    }

    public void AcceptMouseInput(bool accept)
    {
        acceptMouseInput = accept;
    }

    public Entity HandleSelection()
    {
        var positions = GameSystem.EntityManager.GetPositions();
        var mousePos = GetTilePositionAtMouse();

        foreach (Position position in positions)
        {
            if ((position.X == mousePos.X) && (position.Y == mousePos.Y))
                return selection = position.Parent;
        }

        return selection = null;
    }

    public void ProcessInput(InputEvent inputEvent)
    {
        if (MouseIsInBounds() && acceptMouseInput)
            ProcessMouseInput(inputEvent);

        if (acceptKeyboardInput)
        {
            if (GameSystem.Game.isReplay)
                ReplayInput(inputEvent);
            else if (GameSystem.Turn.IsMyTurn())
                ProcessKeyboardInput(inputEvent);
        }
    }

    void ProcessMouseInput(InputEvent inputEvent)
    {
        //Mouse
        if (inputEvent is InputEventMouseButton mouseEvent
            && mouseEvent.IsPressed())
        {
            Action action = null;

            switch ((ButtonList)mouseEvent.ButtonIndex)
            {
                case ButtonList.Left:

                    if (GameSystem.Game.ui.buildUnit.BuildingUnit && GameSystem.Turn.IsMyTurn() && !GameSystem.Game.isReplay)
                        action = CreateAction(GameSystem.Game.ui.buildUnit.UnitToBuild);
                    else
                        HandleSelection();
                    break;


                case ButtonList.Right:
                    
                    if (GameSystem.Game.ui.buildUnit.BuildingUnit)
                        GameSystem.Game.ui.buildUnit.BuildingUnit = false;
                    else if (GameSystem.Turn.IsMyTurn() && selection != null && !GameSystem.Game.isReplay)
                        action = MoveAction(selection);
                    break;

                case ButtonList.Middle://AI stuff, delete later!!!!!                    
                    var t = GameSystem.Turn.IsMyTurn();
                    User pp = User.Player;
                    if (!t) pp = User.Enemy;
                   
                    action = ConvertAction(ai.GetAction(GameSystem.Game.EncodeGameState(), pp), pp);

                    if (GameSystem.Turn.GetTurnCount() == 1)
                    {
                        GD.Print("Hi");
                        var rid = 0;
                        if (GameSystem.Player.GetID() != (int)pp) rid = GameSystem.Player.ResourceEntity.ID;
                        else rid = Enemy.ResourceEntity.ID;
                        action = new CreateAction(7, 7, 1, (int)pp, rid);
                    }
                            

                    break;///////////////////////////////////////////////////////
            }
            if (action != null)
            {
                GameSystem.Turn.TakeTurn(action);
                GameSystem.Game.ui.buildUnit.BuildingUnit = false;
            }
        }
    }

    void ProcessKeyboardInput(InputEvent inputEvent)
    {
        //Keyboard
        if (inputEvent is InputEventKey keyboardEvent
            && keyboardEvent.IsPressed()
            && !keyboardEvent.Echo)
        {
            Action action = null;

            if ((KeyList)keyboardEvent.Scancode == Options.HotkeyPrawn)
                action = CreateAction(Unit.Prawn);

            if ((KeyList)keyboardEvent.Scancode == Options.HotkeyGobbo)
                action = CreateAction(Unit.Gobbo);

            if ((KeyList)keyboardEvent.Scancode == Options.HotkeyStatue)
                action = CreateAction(Unit.Building);

            if ((KeyList)keyboardEvent.Scancode == Options.HotkeyKnight)
                action = CreateAction(Unit.Knight);

            if (action != null)
                GameSystem.Turn.TakeTurn(action);

        }
    }

    public Action ConvertAction(AIAction action, User p)
    {
        if(action is AIMoveAction a)
        {
            foreach(Entity entity in GameSystem.EntityManager.GetEntityList().Keys)
            {
                Position position = GameSystem.EntityManager.GetComponent<Position>(entity);
                if (position != null && position.X == a.unit.X && position.Y == a.unit.Y)
                    return new MoveAction(entity.ID, a.x, a.y);
            }
        }
        else if(action is AIAttackAction b)
        {
            Entity attacker = null, defender = null;
            foreach (Entity entity in GameSystem.EntityManager.GetEntityList().Keys)
            {
                Position position = GameSystem.EntityManager.GetComponent<Position>(entity);
                if (position != null && position.X == b.attacker.X && position.Y == b.attacker.Y)
                    attacker = entity;
                if (position != null && position.X == b.defender.X && position.Y == b.defender.Y)
                    defender = entity;
            }
            return new AttackAction(attacker.ID, defender.ID);
        }
        else if(action is AICreateAction c)
        {
            var rid = 0;
            if (GameSystem.Player.GetID() != (int)p) rid = GameSystem.Player.ResourceEntity.ID;
            else rid = Enemy.ResourceEntity.ID;
            return new CreateAction(c.x, c.y, (int)c.unitType, (int)p, rid);
        }

        return null;
    }

    //Maybe move the logic to Turn
    public void ReplayInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventKey keyboardEvent
        && keyboardEvent.IsPressed())
        {
            Action action = null;

            if ((KeyList)keyboardEvent.Scancode == KeyList.Right)
            {                
                if (GameSystem.Turn.GetReplayCount() == GameSystem.Turn.GetListSize()) return;

                var replayAction = GameSystem.Turn.GetReplayAction(GameSystem.Turn.GetTurnCount() - 1);
                var actionData = (object)replayAction.ReturnData();
                /*if (turn.IsMyTurn())
                    action = replayAction;
                else
                    //game.Rpc("RemoteAction", actionData);
                    action = replayAction;*/
                action = replayAction;
            }

            if ((KeyList)keyboardEvent.Scancode == KeyList.Left)
            {
                if (GameSystem.Turn.GetListSize() > 0)
                {
                    var lastAction = GameSystem.Turn.GetLastAction();
                    GameSystem.Turn.RemoveInvalidAction(lastAction);
                    lastAction.Undo();

                    GameSystem.HandlerManager.ReverseHandlers();
                }
            }

            if (action != null) GameSystem.Turn.TakeTurn(action);
        }
    }

    //Returns a new MoveAction
    Action MoveAction(Entity entity)
    {
        if (processActionInput)
        {
            var id = entity.ID;
            var mousePos = GetTilePositionAtMouse();

            if (GameSystem.Map.IsPassable(mousePos.X, mousePos.Y))
                return new MoveAction(id, mousePos.X, mousePos.Y);
            else
            {
                foreach (Entity e in GameSystem.EntityManager.GetEntityList().Keys)
                {
                    Position clickedEntityPosition = GameSystem.EntityManager.GetComponent<Position>(e);
                    
                    if (clickedEntityPosition != null && clickedEntityPosition.X == mousePos.X && clickedEntityPosition.Y == mousePos.Y)
                    {
                        if (GameSystem.Turn.MovingPlayerOwnsEntity(e) && entity!= e)
                            return new SwapAction(id, e.ID);
                    }
                }
                return new MoveAction(id, mousePos.X, mousePos.Y);
            }
        }
        return null;
    }

    //Returns a new CreateAction
    Action CreateAction(Unit unit)
    {
        if (processActionInput)
        {
            var tilePos = GameSystem.Input.GetTilePositionAtMouse();

            if (GameSystem.Map.IsPassable(tilePos.X, tilePos.Y) && GameSystem.Turn.IsMyTurn())
                return new CreateAction(tilePos.X, tilePos.Y, (int)unit, GameSystem.Player.GetID(), GameSystem.Player.ResourceEntity.ID);

            return null;
        }
        return null;
    }

}