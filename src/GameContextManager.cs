public class GameContextManager
{
    public GameInfo GameInfo { get; private set; }
    public GameContext Context { get; private set; }

    public bool IsReplay { get => GameInfo.GameType == GameType.Replay; }

    public GameContextManager(GameInfo gameInfo)
    {
        GameInfo = gameInfo;
    }

    public void SetGameTypeToReplay()
    {
        if (GameInfo.GameType != GameType.Live) return;
        GameInfo = new GameInfo(GameType.Replay,
                                        GameInfo.ReplayPath, GameInfo.Time, GameInfo.Increment,
                                        GameInfo.TimerType, GameInfo.FirstPlayer, GameInfo.KingCount, GameInfo.Singleplayer);
    }

    public void SetGameOver()
    {
        Context = new GameContext(Context.LocalRematch, Context.RemoteRematch, true);
    }
}