using Godot;
using System.Collections.Generic;
using static GameSystem;

public class GameUI : Control
{
	ConfirmationDialog surrenderConfirmation;
	Button surrenderButton, rematchButton;
	ChatBox chat;
	Control replayPanel;
	Label turnLabel, timer1, timer2, timer1Name, timer2Name;
	OptionsMenu optionsMenu;

	public BuildUnit buildUnit;

	readonly PackedScene chatBoxScene = (PackedScene)GD.Load("res://src/UI/ChatBox.tscn");
	
	readonly List<UILabel> uiLabels = new List<UILabel>();

	bool surrendered = false;

	[Signal]
	public delegate void Surrender();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ShowBehindParent = true;
		chat = (ChatBox)Game.InstantiateChildNode(chatBoxScene, GetNode("Panel"));
        chat.Initialise(GameSystem.Player.Name, 6, 370, 282, 113);

		optionsMenu = (OptionsMenu)GetNode("OptionsMenu");

		buildUnit = (BuildUnit)GetNode("BuildUnit");

		surrenderButton = (Button)GetNode("Panel/SurrenderButton");
		surrenderConfirmation = (ConfirmationDialog)GetNode("Panel/SurrenderButton/SurrenderConfirmation");

		rematchButton = (Button)GetNode("Panel/RematchButton");

        chat.Connect("ChatSignal", GameSystem.Game, "ChatInput");

		replayPanel = (Control)GetNode("Panel/ReplayNode");
		turnLabel = (Label)GetNode("Panel/ReplayNode/TurnCount");

		timer1 = (Label)GetNode("Panel/TimerInfo/Timer1");
		timer2 = (Label)GetNode("Panel/TimerInfo/Timer2");

		timer1Name = (Label)GetNode("Panel/TimerInfo/Timer1/Name");
		timer2Name = (Label)GetNode("Panel/TimerInfo/Timer2/Name");

		optionsMenu.Connect("Toggled", this, nameof(_on_Options_Toggled));

		SetTimerColour();

		CreateLabels();
	}

	public void Initialise()
	{
		if (GameSystem.Game.IsReplay)
		{
            replayPanel.Show();
            rematchButton.Disabled = true;
            rematchButton.Hide();

			var a = (Control)GetNode("Panel/TimerInfo");
			a.Hide();   
		}
	}

	void CreateLabels()
	{
		var vbox = InstantiateVBox(10, 10);
		InstantiateLabel(new UILabelName(), vbox);
		InstantiateLabel(new UILabelHealth(), vbox);
		InstantiateLabel(new UILabelDamage(), vbox);

		var vbox2 = InstantiateVBox(150, 10);
		InstantiateLabel(new UILabelTurn(), vbox2);
		InstantiateLabel(new UILabelResource(), vbox2);

	}

	void SetTimerColour()
	{
		timer1Name.Text = Enemy.Name + ":";
		timer1Name.AddColorOverride("font_color", Options.EnemyColour);
        timer2Name.Text = GameSystem.Player.Name + ":";
		timer2Name.AddColorOverride("font_color", Options.FriendlyColour);
	}

	VBoxContainer InstantiateVBox(int x, int y)
	{
		var vbox = new VBoxContainer();
		vbox.SetPosition(new Vector2(x, y));
		GetNode("Panel").AddChild(vbox);

		return vbox;
	}

	void InstantiateLabel(UILabel label, VBoxContainer vbox)
	{
		uiLabels.Add(label);
		vbox.AddChild(label.Label);
	}

	void UpdateLabels()
	{
        turnLabel.Text = "Turn: " + GameSystem.Game.Turn.GetTurnCount();

		var timeSpan = System.TimeSpan.FromMilliseconds(Enemy.Timer.currentTime);
		timer1.Text = timeSpan.ToString("mm':'ss");

		timeSpan = System.TimeSpan.FromMilliseconds(GameSystem.Player.Timer.currentTime);
		timer2.Text = timeSpan.ToString("mm':'ss");

		foreach (UILabel label in uiLabels)
			label.Update();
	}

	public override void _Process(float delta)
	{
		UpdateLabels();
		if (optionsMenu.ToggledOn)
			ShowBehindParent = false;
		else ShowBehindParent = true;
	}

	public void EnableRematch()
	{
		rematchButton.Disabled = false;
		rematchButton.Show();
	}

	private void _on_SurrenderButton_pressed()
	{
		if (surrendered)
            GameSystem.Game.EndGame();
		else
			surrenderConfirmation.PopupCentered();
	}

	private void _on_SurrenderConfirmation_confirmed()
	{
		DisableSurrender();

        ChatMessage(GameSystem.Player.Name + " surrendered!");
        GameSystem.Game.Rpc("GameResult", Enemy.Name);
	}

	public void DisableSurrender()
	{
		surrenderConfirmation.QueueFree();
		surrendered = true;

		surrenderButton.Text = "Exit";
		EnableRematch();
	}

	public void ChatMessage(string message)
	{
		//chat.AddMessage(message);
        if(!GameSystem.Game.IsSingleplayer)
            chat.Rpc("AddMessage", message);
	}

	public void LocalChatMessage(string message)
	{
		chat.AddMessage(message);
	}

	public void ChatActionLog(Action action)
	{
		string message = "";
		Sprite sprite = new Sprite();
		if(action is MoveAction moveAction)
		{
            /*Name nameComponent = (Name)entityManager.GetComponent(moveAction._entityID, "Name");
			string name = nameComponent.name;

			Owner ownerComponent = (Owner)entityManager.GetComponent(moveAction._entityID, "Owner");
			var owner = ownerComponent.ownedBy;

			string playerName = (owner == User.Player) ? player.GetName() : enemy.GetName();

			message = $"{playerName}'s {name} moved to {moveAction._destinationX}, {moveAction._destinationY}";*/
            Sprite unitSprite = GameSystem.EntityManager.GetComponent<Sprite>(moveAction.EntityID);
			chat.LogMove(unitSprite);
		}
		if (action is AttackAction attackAction)
		{
			/*Name attackerNameComponent = (Name)entityManager.GetComponent(attackAction.attackerID, "Name");
			string attackerName = attackerNameComponent.name;

			Name defenderNameComponent = (Name)entityManager.GetComponent(attackAction.defenderID, "Name");
			string defenderName = defenderNameComponent.name;

			Owner ownerComponent = (Owner)entityManager.GetComponent(attackAction.attackerID, "Owner");
			var owner = ownerComponent.ownedBy;

			Owner defenderOwnerComponent = (Owner)entityManager.GetComponent(attackAction.defenderID, "Owner");
			var defenderOwner = defenderOwnerComponent.ownedBy;

			string playerName = (owner == User.Player) ? player.GetName() : enemy.GetName();
			string enemyName = (owner != User.Player) ? player.GetName() : enemy.GetName();*/

			//message = $"{playerName}'s {attackerName} attacked {playerName}'s {defenderName}";
			if(!attackAction.Killed)
			{
                Sprite attackerSprite = GameSystem.EntityManager.GetComponent<Sprite>(attackAction.AttackerID);
                Sprite defenderSprite = GameSystem.EntityManager.GetComponent<Sprite>(attackAction.DefenderID);
				chat.LogAttack(attackerSprite, defenderSprite);
			}
			else
			{
                Sprite attackerSprite = GameSystem.EntityManager.TryGetComponent<Sprite>(attackAction.AttackerID);
                Sprite defenderSprite = GameSystem.EntityManager.TryGetComponent<Sprite>(attackAction.DefenderID);
				chat.LogAttack(attackerSprite, defenderSprite, true);
			}
		}
		if (action is CreateAction createAction)
		{
            /*Name nameComponent = (Name)entityManager.GetComponent(createAction._createdEntity, "Name");
			string name = nameComponent.name;

			Owner ownerComponent = (Owner)entityManager.GetComponent(createAction._createdEntity, "Owner");
			var owner = ownerComponent.ownedBy;

			string playerName = (owner == User.Player) ? player.GetName() : enemy.GetName();
			sprite = (Sprite)entityManager.GetComponent(createAction._createdEntity, "Sprite");
			message = $"{playerName} created {name} at {createAction.x}, {createAction.y}";*/
            Sprite unitSprite = GameSystem.EntityManager.GetComponent<Sprite>(createAction.CreatedEntity);
			chat.LogCreate(unitSprite);

		}
		if (action is SwapAction swapAction)
		{
            Sprite firstUnitSprite = GameSystem.EntityManager.GetComponent<Sprite>(swapAction.SwappingEntity);
            Sprite secondUnitSprite = GameSystem.EntityManager.GetComponent<Sprite>(swapAction.SwappedEntity);

			chat.LogSwap(firstUnitSprite, secondUnitSprite);
		}
		//chat.AddMessage(message);
		//chat.AddImage(sprite);
	}

	private void _on_RematchButton_pressed()
	{
        GameSystem.Game.Rematch();
		rematchButton.Disabled = true;
	}

	private void _on_Button_pressed()
	{
        GameSystem.Sound.Mute();
	}

	private void _on_ReplayForward_pressed()
	{
		var inputEvent = new InputEventKey();
		inputEvent.Pressed = true;
		inputEvent.Scancode = (int)KeyList.Right;

        GameSystem.Input.ReplayInput(inputEvent);
	}


	private void _on_ReplayBackward_pressed()
	{
		var inputEvent = new InputEventKey();
		inputEvent.Pressed = true;
		inputEvent.Scancode = (int)KeyList.Left;

        GameSystem.Input.ReplayInput(inputEvent);
	}

	private void _on_Options_pressed()
	{
		optionsMenu.ToggleMenu();
	}

	void _on_Options_Toggled()
	{
		SetTimerColour();
	}
}
