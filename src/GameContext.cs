public struct GameContext
{
    public bool LocalRematch { get; private set; }
    public bool RemoteRematch { get; private set; }
    public bool GameOver { get; private set; }

    public GameContext(bool localRematch = false, bool remoteRematch = false, bool gameOver = false)
    {
        LocalRematch = localRematch;
        RemoteRematch = remoteRematch;
        GameOver = gameOver;
    }
}