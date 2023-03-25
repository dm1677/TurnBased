using Godot;
using System;
using System.Collections.Generic;

public class GameSetup : Node
{
	const int _VERSION = 6;

	[Signal]
	public delegate void ConnectionSuccess();
	[Signal]
	public delegate void ConnectionFailed();
	[Signal]
	public delegate void ConnectedAsServer();
	[Signal]
	public delegate void ConnectedAsClient();
	[Signal]
	public delegate void PeerDisconnected();
	[Signal]
	public delegate void PlayerInfoChanged();

	readonly PackedScene gameNode = (PackedScene)GD.Load("res://src/Scenes/Game.tscn");
	readonly PackedScene lobbyNode = (PackedScene)GD.Load("res://src/UI/Lobby.tscn");

	public readonly Sync sync = new Sync();

	private Lobby lobby;
	private Game game;

	private int gameID = 0;

	const int port = 6010;
	private readonly UPNP upnp = new UPNP();
	private NetworkedMultiplayerENet connection;

	public ICollection<PlayerInfo> PlayerInfo { private set; get; } = new HashSet<PlayerInfo>();
	private PlayerInfo myInfo;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Options.ReadFromFile();

		SetLobby();
		AddChild(sync);

		upnp.Discover(2000, 2, "InternetGatewayDevice");
		upnp.AddPortMapping(port);

		GetTree().Connect("connected_to_server", this, nameof(OnConnectedToServer));
		GetTree().Connect("connection_failed", this, nameof(OnConnectionFailed));
		GetTree().Connect("network_peer_connected", this, nameof(OnNetworkPeerConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(OnNetworkPeerDisconnected));
		GetTree().Connect("server_disconnected", this, nameof(OnServerDisconnected));
	}

	void SetLobby()
	{
		AddChild(lobbyNode.Instance());
		lobby = (Lobby)GetNode("Lobby");

		lobby.Connect("Create", this, nameof(HostGame));
		lobby.Connect("Join", this, nameof(JoinGame));
		lobby.Connect("CancelCreate", this, nameof(EndSession));

		lobby.Connect("StartGame", this, nameof(OnStartGamePressed));

		lobby.Connect("Replay", this, nameof(StartGame));
		lobby.Connect("Singleplayer", this, nameof(StartGame));
	}

	private void ClearLobby()
	{
		if (lobby != null)
		{
			GetNode("Lobby").QueueFree();
			lobby = null;
		}
	}

	public void EndSession()
	{
		EndConnection();
		gameID = 0;

		if (lobby == null)
			SetLobby();

		PlayerInfo.Clear();
		EmitSignal(nameof(PlayerInfoChanged));
	}


	void EndConnection()
	{
		if (connection != null && connection.GetConnectionStatus() != NetworkedMultiplayerPeer.ConnectionStatus.Disconnected)
			connection.CloseConnection();

		GetTree().NetworkPeer = null;

		EmitSignal("ConnectionFailed");
	}

	void OnServerDisconnected()
	{
		EndConnection();
	}

	void OnNetworkPeerDisconnected(int id)
	{
		foreach (PlayerInfo player in PlayerInfo)
		{
			if (player.ID == id)
			{
				PlayerInfo.Remove(player);
				EmitSignal(nameof(PlayerInfoChanged));
				break;
			}
		}

		EmitSignal(nameof(PeerDisconnected));

		sync.ClearList();
	}

	void OnConnectionFailed()
	{
		Logging.Log("Connection failed.");
		EmitSignal(nameof(ConnectionFailed));
	}

	public void OnNetworkPeerConnected(int id)
	{
		Rpc("ValidateVersion", (object)_VERSION);

		var data = new string[2] { myInfo.Name, myInfo.ID.ToString() };
		Rpc("RegisterPlayerInfo", (object)data);

		EmitSignal(nameof(ConnectionSuccess));
	}

	void OnStartGamePressed()
	{
		var gameInfo = lobby.GetGameInfo().Serialise();
		//var gameInfo = new GameInfo(false, null, 40, 4, Timer.TimerType.GameTimer).Serialise();
		//StartGame(gameInfo);

		Rpc("StartGame", (object)gameInfo);
	}

	void OnConnectedToServer()
	{
		//EmitSignal(nameof(ConnectionSuccess));
		//Logging.Log("Connection Success");
	}

	[Remote]
	void ValidateVersion(int version)
	{
		if (version != _VERSION)
		{
			EndSession();
			Logging.Log("Game versions incompatible!");
		}
	}

	[Remote]
	void RegisterPlayerInfo(string[] info)
	{
		PlayerInfo playerInfo = new PlayerInfo(info[0], info[1].ToInt());
		this.PlayerInfo.Add(playerInfo);

		EmitSignal(nameof(PlayerInfoChanged));
	}

	void SetMyInfo()
	{
		myInfo = lobby.GetPlayerInfo();
		PlayerInfo.Add(myInfo);
		EmitSignal(nameof(PlayerInfoChanged));
	}

	void HostGame()
	{
		var host = new NetworkedMultiplayerENet();
		host.CreateServer(port, 1);
		GetTree().NetworkPeer = host;

		connection = host;

		SetMyInfo();
	}

	void JoinGame(string ip)
	{
		var client = new NetworkedMultiplayerENet();

		client.CreateClient(ip, port);
		GetTree().NetworkPeer = client;

		connection = client;

		SetMyInfo();
	}

	[RemoteSync]
	public void StartGame(object[] data)
	{
		/*if (game != null && !game.IsQueuedForDeletion())
			game.Free();//what's going on here? "can't access pointer to disposed object"*/

		//GC.Collect();

		var gameInfo = GameInfo.Deserialise(data);
		gameInfo.SwapFirstPlayer();

		/*if (GetTree().IsNetworkServer())
			EmitSignal(nameof(ConnectedAsServer));
		else
			EmitSignal(nameof(ConnectedAsClient));//do these signals even do anything any more?*/

		game = (Game)Game.InstantiateChildNode(gameNode, this);
		game.Name = gameID++.ToString();
		Logging.Log("Game ID: " + gameID);
		game.Initialise(gameInfo, PlayerInfo);

		ClearLobby();
	}

	public override void _ExitTree()
	{
		upnp.DeletePortMapping(port);
	}
}
