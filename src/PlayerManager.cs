using System.Collections.Generic;
using System;

public class PlayerManager
{
    public Player LocalPlayer { get; private set; }
    public Player RemotePlayer { get; private set; }

    public PlayerManager(GameContextManager contextManager, ICollection<PlayerInfo> playerInfo)
    {
        CreatePlayers(contextManager, playerInfo);
        CreateTimer(contextManager, LocalPlayer);
        CreateTimer(contextManager, RemotePlayer);
    }

    void CreatePlayers(GameContextManager contextManager, ICollection<PlayerInfo> playerInfo)
    {
        var listSize = playerInfo.Count;
        if (listSize > 1)
        {
            PlayerInfo[] array = new PlayerInfo[listSize];
            playerInfo.CopyTo(array, 0);

            LocalPlayer = CreatePlayer(contextManager, array[0]);
            RemotePlayer = CreatePlayer(contextManager, array[1]);
        }
        else
        {
            LocalPlayer = CreatePlayer(contextManager, new PlayerInfo("Player One", 1));
            RemotePlayer = CreatePlayer(contextManager, new PlayerInfo("Player Two", 2));
        }
    }

    Player CreatePlayer(GameContextManager contextManager, PlayerInfo playerInfo)
    {
        var name = playerInfo.Name;
        var id = playerInfo.ID;

        return new Player(id, name, contextManager.GameInfo.FirstPlayer);
    }

    void CreateTimer(GameContextManager contextManager, Player player)
    {
        var timerEntity = ComponentFactory.Instance().CreateTimer(contextManager.GameInfo.TimerType,
                                                                  contextManager.GameInfo.Time,
                                                                  contextManager.GameInfo.Increment,
                                                                  player.ID);
        player.SetTimer(timerEntity);
    }
}