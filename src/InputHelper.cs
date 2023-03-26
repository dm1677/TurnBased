using System;
using Godot;
using static GameSystem;

public class InputHelper
{
    Entity selection;

    readonly int _tileSize;

    bool acceptKeyboardInput = true;
    bool acceptMouseInput = true;
    public bool processActionInput = true;

    AIManager ai = new AIManager();

    public InputHelper()
    {
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
            if (GameSystem.Game.IsReplay)
                ReplayInput(inputEvent);
            else if (GameSystem.Game.Turn.IsMyTurn())
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

                    if (GameSystem.Game.UI.buildUnit.BuildingUnit && GameSystem.Game.Turn.IsMyTurn() && !GameSystem.Game.IsReplay)
                        action = CreateAction(GameSystem.Game.UI.buildUnit.UnitToBuild);
                    else
                        HandleSelection();
                    break;


                case ButtonList.Right:
                    
                    if (GameSystem.Game.UI.buildUnit.BuildingUnit)
                        GameSystem.Game.UI.buildUnit.BuildingUnit = false;
                    else if (GameSystem.Game.Turn.IsMyTurn() && selection != null && !GameSystem.Game.IsReplay && processActionInput)
                        action = RightClickWithEntitySelected(selection);
                    break;

                case ButtonList.Middle://AI stuff, delete later!!!!!                    
                    var t = GameSystem.Game.Turn.IsMyTurn();
                    User pp = User.Player;
                    if (!t) pp = User.Enemy;
                   
                    action = ConvertAction(ai.GetAction(GameSystem.Game.EncodeGameState(), pp), pp);

                    if (GameSystem.Game.Turn.GetTurnCount() == 1)
                    {
                        Logging.Log("Hi");
                        var rid = 0;
                        if (GameSystem.Player.ID != (int)pp) rid = GameSystem.Player.ResourceEntity.ID;
                        else rid = Enemy.ResourceEntity.ID;
                        action = new CreateAction(7, 7, 1, (int)pp, rid);
                    }
                            

                    break;///////////////////////////////////////////////////////
            }
            if (action != null)
            {
                GameSystem.Game.Turn.TakeTurn(action);
                GameSystem.Game.UI.buildUnit.BuildingUnit = false;
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
                GameSystem.Game.Turn.TakeTurn(action);

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
            if (GameSystem.Player.ID != (int)p) rid = GameSystem.Player.ResourceEntity.ID;
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
            if ((KeyList)keyboardEvent.Scancode == KeyList.Right)
                GameSystem.Game.Turn.AdvanceReplay();

            if ((KeyList)keyboardEvent.Scancode == KeyList.Left)
                GameSystem.Game.Turn.ReverseReplay();
        }
    }

    Action RightClickWithEntitySelected(Entity entity)
    {
        var mousePos = GetTilePositionAtMouse();

        foreach (Entity otherEntity in GameSystem.EntityManager.GetEntityList().Keys)
        {
            if (otherEntity == entity) continue;
            Position otherEntityPosition = GameSystem.EntityManager.GetComponent<Position>(otherEntity);

            if (otherEntityPosition == null
                || otherEntityPosition.X != mousePos.X
                || otherEntityPosition.Y != mousePos.Y)
                continue;

            Owner owner = GameSystem.EntityManager.GetComponent<Owner>(otherEntity);
            if (GameSystem.Game.Turn.MovingPlayerOwnsEntity(otherEntity))
                return new SwapAction(entity.ID, otherEntity.ID);
            else if (owner != null && owner.ownedBy != User.Neutral)
                return new AttackAction(entity.ID, otherEntity.ID);
        }
        return new MoveAction(entity.ID, mousePos.X, mousePos.Y);
    }

    //Returns a new CreateAction
    Action CreateAction(Unit unit)
    {
        if (processActionInput)
        {
            var tilePos = GameSystem.Input.GetTilePositionAtMouse();

            if (GameSystem.Map.IsPassable(tilePos.X, tilePos.Y) && GameSystem.Game.Turn.IsMyTurn())
                return new CreateAction(tilePos.X, tilePos.Y, (int)unit, GameSystem.Player.ID, GameSystem.Player.ResourceEntity.ID);

            return null;
        }
        return null;
    }

}