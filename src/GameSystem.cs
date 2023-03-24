public static class GameSystem
{
	public static Game Game { get; private set; }

	public static Player Player { get => Game.PlayerManager.LocalPlayer; }
	public static Player Enemy { get => Game.PlayerManager.RemotePlayer; } 
	public static Map Map { get; private set; }
	public static EntityManager EntityManager { get; private set; }
	public static InputHelper Input { get; private set; }
	public static Sound Sound { get; private set; }

	public static void InitialiseGameSystem(Game game, int mapWidth, int mapHeight)
	{
		Game = game;

		EntityManager = new EntityManager();
		Map = new Map(mapWidth, mapHeight);
		Input = new InputHelper();
        Sound = new Sound();

		ComponentFactory.Instance();
		
	} 
}