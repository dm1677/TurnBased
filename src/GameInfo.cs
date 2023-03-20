using System;

public struct GameInfo
{
    public GameInfo(bool replay, string replayPath, int time, int increment, TimerType timerType, bool hostIsFirstPlayer, int kingCount, bool singleplayer = false)
    {
        Replay = replay;

        if (replayPath == null)
            ReplayPath = AppDomain.CurrentDomain.BaseDirectory + @"\Replays\LastReplay.tbr";
        else
            ReplayPath = replayPath;

        TimerType = timerType;
        Time = time;
        Increment = increment;
        FirstPlayer = hostIsFirstPlayer;
        KingCount = kingCount;
        Singleplayer = singleplayer;
    }

    public bool Replay { get; private set; }
    public string ReplayPath { get; private set; }
    public TimerType TimerType { get; private set; }
    public int Time { get; private set; }
    public int Increment { get; private set; }
    public bool FirstPlayer { get; private set; }
    public int KingCount { get; private set; }
    public bool Singleplayer { get; private set; }

    public object[] Serialise()
    {
        return new object[8] { Replay.ToString(), ReplayPath, Time, Increment, (int)TimerType, FirstPlayer.ToString(), KingCount, Singleplayer.ToString() };
    }

    public static GameInfo Deserialise(object[] data)
    {
        bool replay = (string)data[0] != "False";
        bool hostIsFirstPlayer = (string)data[5] != "False";
        bool singleplayer = (string)data[7] != "False";

        return new GameInfo(replay, (string)data[1], (int)data[2], (int)data[3], (TimerType)data[4], hostIsFirstPlayer, (int)data[6], singleplayer);
    }

    public void SwapFirstPlayer()
    {
        FirstPlayer = !FirstPlayer;
    }
}