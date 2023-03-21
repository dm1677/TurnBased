using Godot;
using System;

public class Lobby : Control
{
	[Signal]
	public delegate void Create();

	[Signal]
	public delegate void Join(string ip);

	[Signal]
	public delegate void CancelCreate();

	[Signal]
	public delegate void Singleplayer();

	[Signal]
	public delegate void Replay();

	[Signal]
	public delegate void StartGame();

	LineEdit ip, inputname;
	SpinBox timerTime, timerIncrement;
	NinePatchRect panel, setupPanel, ipPanel, timerPanel;
	Button create, join, cancel, singleplayer, replay, startgame;
	Label standby, status;
	FileDialog replayfile;
	OptionButton timerTypeSelect, kingCount;
	Node2D background;

	Vector2 backgroundPosition;

	readonly Random random = new Random();

	ChatBox chat;

	string playerName;
	const string _invalidNameLength = "Invalid name length!";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		panel = (NinePatchRect)GetNode("LobbyPanel");
		create = (Button)GetNode("LobbyPanel/Create");
		join = (Button)GetNode("LobbyPanel/Join");
		standby = (Label)GetNode("LobbyPanel/Standby");
		inputname = (LineEdit)GetNode("LobbyPanel/InputName");

		setupPanel = (NinePatchRect)GetNode("SetupPanel");
		replay = (Button)GetNode("SetupPanel/ReplayButton");
		replayfile = (FileDialog)GetNode("SetupPanel/ReplayButton/ReplayFile");
		startgame = (Button)GetNode("SetupPanel/StartGameButton");
		cancel = (Button)GetNode("SetupPanel/CancelCreate");

		ipPanel = (NinePatchRect)GetNode("IPPanel");
		ip = (LineEdit)GetNode("IPPanel/InputIP");
		status = (Label)GetNode("IPPanel/Status");

		timerPanel = (NinePatchRect)GetNode("SetupPanel/TimerPanel");

		timerTypeSelect = (OptionButton)GetNode("SetupPanel/TimerPanel/TimerType/TimerTypeSelect");
		timerTypeSelect.AddItem("Game");
		timerTypeSelect.AddItem("Turn");

		timerTime = (SpinBox)GetNode("SetupPanel/TimerPanel/Time/Input");
		timerIncrement = (SpinBox)GetNode("SetupPanel/TimerPanel/Increment/Input");

		kingCount = GetNode<OptionButton>("SetupPanel/TimerPanel/KingCount/KingCountSelect");

		chat = (ChatBox)setupPanel.GetNode("ChatBox");

		background = (Node2D)GetNode("Background");
		backgroundPosition = background.Position;

		GetParent().Connect("ConnectionSuccess", this, nameof(OnConnectionSuccess));
		GetParent().Connect("ConnectionFailed", this, nameof(OnConnectionFail));
		GetParent().Connect("PeerDisconnected", this, nameof(OnPeerDisconnected));
	}

	public override void _Process(float delta)
	{
		if(background.Position != backgroundPosition)
		{
			background.Position = new Vector2(Mathf.MoveToward(background.Position.x, backgroundPosition.x, 0.2f),
											  Mathf.MoveToward(background.Position.y, backgroundPosition.y, 0.2f));
		}
		else
		{
			backgroundPosition.x = random.Next(0, 951); //-197
			backgroundPosition.y = random.Next(100, 531);//30
		}

		Update();
	}

	public PlayerInfo GetPlayerInfo()
	{
		return new PlayerInfo(playerName, GetTree().GetNetworkUniqueId());
	}

	[Remote]
	public GameInfo GetGameInfo()
	{
		TimerType timerType = (timerTypeSelect.Text == "Game") ? TimerType.GameTimer : TimerType.TurnTimer;

		bool firstPlayer = (random.Next(2) == 1);

		return new GameInfo(GameType.Live, null, (int)timerTime.Value, (int)timerIncrement.Value, timerType, firstPlayer, kingCount.Selected);
	}

	void OnConnectionSuccess()
	{
		setupPanel.Show();
		ipPanel.Hide();

		if (GetTree().GetNetworkUniqueId() == 1)
			startgame.Disabled = false;
	}

	void OnConnectionFail()
	{
		setupPanel.Hide();
		ipPanel.Hide();
		panel.Show();

		startgame.Disabled = true;
		standby.Text = "";
		status.Text = "";
	}

	private void _on_CreateButton_pressed()
	{
		var checkNameLength = CheckSetNameLength(inputname.Text);

		if (checkNameLength)
		{
			EmitSignal(nameof(Create));
			panel.Hide();
			setupPanel.Show();
			timerPanel.Show();
			EnableTimerPanel();
		}

		if (!checkNameLength)
			standby.Text = _invalidNameLength;
	}

	private void _on_JoinButton_pressed()
	{
		var checkNameLength = CheckSetNameLength(inputname.Text);
		if (checkNameLength)
		{
			ipPanel.Show();
			panel.Hide();
		}
		else standby.Text = _invalidNameLength;

		//timerPanel.Hide();
		DisableTimerPanel();
	}

	bool CheckSetNameLength(string str)
	{
		if (str.Length <= 0) return false;
		if (str.Length >= 15) return false;

		playerName = str;
		chat.SetPlayerName(playerName);
		return true;
	}

	private void _on_CancelCreate_pressed()
	{
		panel.Show();
		setupPanel.Hide();
		EmitSignal(nameof(CancelCreate));
	}

	private void _on_ReplayButton_pressed()
	{
		replayfile.PopupCentered();
	}

	private void OnPeerDisconnected()
	{
		startgame.Disabled = true;
	}

	private void _on_ReplayFile_file_selected(String path)
	{
		var gameInfo = new GameInfo(GameType.Replay, path, 30, 0, 0, true, 2).Serialise();

		EmitSignal(nameof(Replay), (object)gameInfo);
	}

	private void _on_Start_Game_pressed()
	{
		EmitSignal(nameof(StartGame));
	}

	private void _on_ConfirmIP_pressed()
	{
		var checkValidIP = StringExtensions.IsValidIPAddress(ip.Text);

		if (checkValidIP)
		{
			EmitSignal(nameof(Join), ip.Text);
			standby.Text = "Attempting to join game...";
			status.Text = "Connecting...";
		}
		else
		{
			status.Text = "Invalid IP Address!";
		}
	}

	private void _on_Cancel_pressed()
	{
		EmitSignal("CancelCreate");
		ipPanel.Hide();
		panel.Show();
		status.Text = "";
	}

	private void _on_LobbyPanel_visibility_changed()
	{
		chat.ClearChat();
	}    

	private void DisableTimerPanel()
	{
		timerTypeSelect.Disabled = true;
		timerTime.Editable = false;
		timerIncrement.Editable = false;
		kingCount.Disabled = true;
	}

	private void EnableTimerPanel()
	{
		timerTypeSelect.Disabled = false;
		timerTime.Editable = true;
		timerIncrement.Editable = true;
		kingCount.Disabled = false;
	}

	private void TimerDataChanged()
	{
		Rpc(nameof(UpdateTimerInfo), (object)GetGameInfo().Serialise());
	}

	[Puppet]
	private void UpdateTimerInfo(object[] data)
	{
		var gameInfo = GameInfo.Deserialise(data);

		timerTypeSelect.Selected = (int)gameInfo.TimerType;
		timerTime.Value = gameInfo.Time;
		timerIncrement.Value = gameInfo.Increment;
		kingCount.Selected = gameInfo.KingCount;
	}

	private void _on_Input_value_changed(float value)
	{
		TimerDataChanged();
	}

	private void _on_TimerTypeSelect_item_selected(int index)
	{
		TimerDataChanged();
	}

	private void _on_IncrementInput_value_changed(float value)
	{
		TimerDataChanged();
	}

	private void _on_ExitButton_pressed()
	{
		GetTree().Quit();
	}

	private void _on_Singleplayer_pressed()
	{
		//var gameInfo = new GameInfo(false, null, 30, 0, TimerType.GameTimer, false, 2, true).Serialise();
		EmitSignal(nameof(Singleplayer));
	}

	private void _on_KingCount_item_selected(int index)
	{
		TimerDataChanged();
	}

}
