using Godot;
using System.Collections.Generic;
using static GameSystem;

public interface IObserver
{
	void EntityAdded(Entity entity);
	void EntityRemoved(Entity entity);
}

public class Render : Node2D, IObserver
{
	private int tileSize, playerID;
	private bool showAllUnitMovement = false;

	public static readonly Color NullModulate = new Color(1, 1, 1, 1);

	readonly Texture selectorTexture = (Texture)GD.Load("res://assets/UI/selector.png");
	readonly Texture enemyUnitHighlight = (Texture)GD.Load("res://assets/UI/selectedEnemy.png");
	readonly Texture passableTileHighlight = (Texture)GD.Load("res://assets/UI/selected.png");
	readonly Texture selected = (Texture)GD.Load("res://assets/UI/selectedNeutral.png");

	readonly List<Vector2> movementPreview = new List<Vector2>();
	readonly List<Vector2> attackPreview = new List<Vector2>();
	readonly List<RenderEntity> renderEntities = new List<RenderEntity>();

	Texture[] spriteSheet;

	Vector2 lastMoveFirst;
	Vector2 lastMoveSecond;

	Entity lastSelection;

	int renderTurn = GameSystem.Game.Turn.GetTurnCount();    

	public enum HealthBarMode
	{
		showNone,
		showAll,
		showDamaged
	}

	public override void _Ready()
	{
		tileSize = GameSystem.Game.GetTileSize();
		playerID = GameSystem.Player.ID;

		GetTextures();

		GameSystem.EntityManager.Register(this);
		foreach(Entity entity in GameSystem.EntityManager.GetEntityList().Keys)
		{
			EntityAdded(entity);
		}
	}

	void GetTextures()
	{
		//replace this with something prettier
		spriteSheet = new Texture[7];

		spriteSheet[0] = (Texture)GD.Load("res://assets/sprites/tiles/sandstone_light.png");
		spriteSheet[1] = (Texture)GD.Load("res://assets/sprites/tree.png");
		spriteSheet[2] = (Texture)GD.Load("res://assets/sprites/king.png");
		spriteSheet[3] = (Texture)GD.Load("res://assets/sprites/gobbo_purple.png");
		spriteSheet[4] = (Texture)GD.Load("res://assets/sprites/unit.png");
		spriteSheet[5] = (Texture)GD.Load("res://assets/sprites/newstatue_purple.png");
		spriteSheet[6] = (Texture)GD.Load("res://assets/sprites/unit2.png");
	}

	public override void _Draw()
	{
		DrawTileSprites();
		DrawLastMove();
		DrawMovementPreview();

		DrawSelector();

		DrawSpriteAtMouse();
	}
   
	public override void _Process(float delta)
	{
		foreach (var e in renderEntities)
		{
			if (e.GetParent() == null)
				AddChild(e);
		}

		UpdateRenderTurn();
		UpdateSelection();
		GetLastMove();
		

		Update();
	}

	void UpdateRenderTurn()
	{
		if (GameSystem.Game.Turn.GetTurnCount() == renderTurn)
			return;

		renderTurn = GameSystem.Game.Turn.GetTurnCount();

		if (GameSystem.Game.Turn.IsMyTurn())
			GetMovementPreview(lastSelection);
		else
			ClearPreviews();
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (Godot.Input.IsKeyPressed((int)KeyList.Control))
			showAllUnitMovement = true;
		else
			showAllUnitMovement = false;
	}

	public void EntityAdded(Entity entity)
	{
		var renderEntity = new RenderEntity();
		renderEntity.Initialise(entity);
		renderEntities.Add(renderEntity);
	}

	public void EntityRemoved(Entity entity)
	{
		foreach(RenderEntity renderEntity in renderEntities)
		{
			if (renderEntity.entity == entity)
			{
				if (renderEntity == null) continue;
				renderEntities.Remove(renderEntity);
				renderEntity.QueueFree();
				break;
			}
		}
	}

	void UpdateSelection()
	{
		if (GameSystem.Input.GetSelection() == lastSelection) return;

		lastSelection = GameSystem.Input.GetSelection();
		if (lastSelection == null || !GameSystem.Game.Turn.IsMyTurn())
		{
			ClearPreviews();
			return;
		}

		GetMovementPreview(lastSelection);
	}

	void ClearPreviews()
	{
		movementPreview.Clear();
		attackPreview.Clear();
	}

	void GetMovementPreview(Entity entity)
	{
		if (entity == null || entity.QueuedForDeletion) return;
		ClearPreviews();

		Movement movement = GameSystem.EntityManager.GetComponent<Movement>(entity);
		Position position = GameSystem.EntityManager.GetComponent<Position>(entity);
		Owner owner    = GameSystem.EntityManager.GetComponent<Owner>(entity);

		if (owner != null && owner.ownedBy != (User)playerID)
			return;

		//attacks
		Weapon weapon = GameSystem.EntityManager.GetComponent<Weapon>(entity);
		if (weapon == null) return;

		var list = MovementHandler.Attack(entity, weapon.attackType);
		foreach (Entity e in list)
		{
			Position pos = GameSystem.EntityManager.GetComponent<Position>(e);
			if (pos == null) continue;
			attackPreview.Add(new Vector2(pos.X * tileSize, pos.Y * tileSize));
		}
		//attacks

		if (movement == null || position == null) return;

		for (int y = 0; y < GameSystem.Map.Height; y++)
		{
			for (int x = 0; x < GameSystem.Map.Width; x++)
			{
				if (MovementHandler.CheckMovement(
						new Coords(x, y),
						new Coords(position.X, position.Y),
						movement.speed,
						movement.movementType))
				{
					movementPreview.Add(new Vector2(x * tileSize, y * tileSize));
				}
			}
		}
		movementPreview.AddRange(GetSwapPreview(entity));
	}

	//Highlights the tile the mouse is currently hovering over
	void DrawSelector()
	{
		var mousePos = GameSystem.Input.GetClampedMousePosition();

		if ((mousePos.X < (GameSystem.Map.Width * tileSize)) &&
			(mousePos.Y < (GameSystem.Map.Height * tileSize)))
		{
			Rect2 rect = new Rect2(mousePos.X, mousePos.Y, tileSize, tileSize);
			DrawTextureRect(selectorTexture, rect, false);
		}
	}
	
	//Retrieves all sprite components and draws them to the screen
	void DrawTileSprites()
	{
		for (int y = 0; y < GameSystem.Map.Height; y++)
		{
			for (int x = 0; x < GameSystem.Map.Width; x++)
			{
				var index = GameSystem.Map.GetTile(x, y).SpriteIndex;
				DrawTexture(spriteSheet[index], new Vector2(x * tileSize, y * tileSize));
			}
		}
	}

	List<Vector2> GetSwapPreview(Entity entity)
	{
		List<Vector2> list = new List<Vector2>();

		Swap swap = GameSystem.EntityManager.GetComponent<Swap>(entity);
		if (swap != null)
		{
			foreach (Entity e in GameSystem.EntityManager.GetEntityList().Keys)
			{
				if (!PlayerCanUseEntity(e)) continue;

				Position position = GameSystem.EntityManager.GetComponent<Position>(e);
				if (position == null) continue;

				list.Add(new Vector2(position.X * tileSize, position.Y * tileSize));
			}
		}
		else
		{
			foreach (Entity e in GameSystem.EntityManager.GetEntityList().Keys)
			{
				if (!PlayerCanUseEntity(e)) continue;

				Position position = GameSystem.EntityManager.GetComponent<Position>(e);
				if (position == null) continue;
				Swap swap2 = GameSystem.EntityManager.GetComponent<Swap>(e);
				if (swap2 == null) continue;

				list.Add(new Vector2(position.X * tileSize, position.Y * tileSize));
				//DrawTexture(_passableTileHighlight, new Vector2(position.x * _tileSize, position.y * _tileSize));
			}
		}

		return list;
	}

	//Returns true if the entity is owned by the player, and it is the player's turn
	bool PlayerCanUseEntity(Entity entity)
	{
		Owner owner = GameSystem.EntityManager.GetComponent<Owner>(entity);
		User ownedBy = owner.ownedBy;

		if (entity != null
		 && ownedBy == (User)playerID
		 && GameSystem.Game.Turn.IsMyTurn())
			return true;
		else return false;
	}

	void DrawMovementPreview()
	{
		foreach(Vector2 vec in movementPreview)
		{
			DrawTexture(passableTileHighlight, vec);
		}

		foreach(Vector2 vec in attackPreview)
		{
			DrawTexture(enemyUnitHighlight, vec);
		}
	}

	void DrawSpriteAtMouse()
	{
		if (!GameSystem.Game.UI.buildUnit.BuildingUnit) return;
		
		var mousePos = GameSystem.Input.GetClampedMousePosition();
		var sprite = (Texture)GD.Load(GameSystem.Game.UI.buildUnit.UnitSprite);
		var color = new Color(1, 1, 1);

		var tilePos = GameSystem.Input.GetTilePositionAtMouse();
		if (!GameSystem.Map.IsPassable(tilePos.X, tilePos.Y))
			color = Options.EnemyColour;

		if ((mousePos.X < (GameSystem.Map.Width * tileSize)) &&
		(mousePos.Y < (GameSystem.Map.Height * tileSize)))
		{
			var pos = new Vector2(mousePos.X, mousePos.Y);
			DrawTexture(sprite, pos, color);
		}
		
	}
 
	void GetLastMove()
	{
		//if (GameSystem.Turn.GetListSize() == 0) return;

		//var action = GameSystem.Turn.GetLastAction();
		Action action = GameSystem.Game.Turn.GetUpdatedAction();
		if (action == null) return;

		if (action is CreateAction createAction)
		{
			Position position = GameSystem.EntityManager.GetComponent<Position>(createAction.CreatedEntity);

			lastMoveFirst = new Vector2(position.X * +tileSize, position.Y * tileSize);
			lastMoveSecond = new Vector2(-1,-1);
		}
		if (action is MoveAction moveAction)
		{
			lastMoveFirst = new Vector2(moveAction.FromX * tileSize, moveAction.FromY * tileSize);
			lastMoveSecond = new Vector2(moveAction.DestinationX * tileSize, moveAction.DestinationY * tileSize);
		}
		if (action is AttackAction attackAction)
		{
			Position attackerPosition = GameSystem.EntityManager.GetComponent<Position>(attackAction.AttackerID);
			Position defenderPosition = GameSystem.EntityManager.TryGetComponent<Position>(attackAction.DefenderID);

			lastMoveFirst = new Vector2(attackerPosition.X * tileSize, attackerPosition.Y * tileSize);

			if (defenderPosition == null) lastMoveSecond = new Vector2(-1,-1);

			lastMoveSecond = new Vector2(defenderPosition.X * tileSize, defenderPosition.Y * tileSize);
		}
		if (action is SwapAction swapAction)
		{
			Position swapperPosition = GameSystem.EntityManager.GetComponent<Position>(swapAction.SwappingEntity);
			Position swappedPosition = GameSystem.EntityManager.GetComponent<Position>(swapAction.SwappedEntity);

			lastMoveFirst = new Vector2(swapperPosition.X * tileSize, swapperPosition.Y * tileSize);
			lastMoveSecond = new Vector2(swappedPosition.X * tileSize, swappedPosition.Y * tileSize);
		}
	}

	void DrawLastMove()
	{
		if (lastMoveFirst.x != -1 && lastMoveFirst.y != -1)
			DrawTexture(selected, lastMoveFirst, Options.neutralColour);

		if (lastMoveSecond.x != -1 && lastMoveSecond.y != -1)
			DrawTexture(selected, lastMoveSecond, Options.neutralColour);
	}    
}
