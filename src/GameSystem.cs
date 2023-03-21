using System.Collections.Generic;

public static class GameSystem
{
	public static Game Game { get; private set; }

	public static Player Player { get; private set; }
    public static Player Enemy { get; private set; }
    public static Map Map { get; private set; }
	public static EntityManager EntityManager { get; private set; }
	public static InputHelper Input { get; private set; }
    public static Sound Sound { get; private set; }

	public static void InitialiseGameSystem(Game g, int mapWidth, int mapHeight, HashSet<PlayerInfo> playerInfo)
	{
		Game = g;

        CreatePlayers(playerInfo);
		EntityManager = new EntityManager();
		Map = new Map(mapWidth, mapHeight);
		Input = new InputHelper();
        Sound = new Sound();

		ComponentFactory.Instance();
    }

    static void CreatePlayers(HashSet<PlayerInfo> playerInfoList)
    {
        var listSize = playerInfoList.Count;
        if (listSize > 1)
        {
            PlayerInfo[] array = new PlayerInfo[listSize];
            playerInfoList.CopyTo(array);

            Player = CreatePlayer(array[0]);
            Enemy = CreatePlayer(array[1]);
        }
        else
        {
            Player = CreatePlayer(new PlayerInfo("Player One", 1));
            Enemy = CreatePlayer(new PlayerInfo("Player Two", 2));
        }
    }

    static Player CreatePlayer(PlayerInfo playerInfo)
    {
        var name = playerInfo.Name;
        var id = playerInfo.ID;

        return new Player(id, name);
    }    
}