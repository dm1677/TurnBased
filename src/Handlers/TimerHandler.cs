using System.Collections.Generic;
using static GameSystem;

public class TimerHandler : IHandler
{
    readonly HashSet<Player> players = new HashSet<Player>();

    public TimerHandler()
    {
        players.Add(GameSystem.Game.Player);
        players.Add(GameSystem.Game.Enemy);
    }

    public bool Process()
    {
        if (!GameSystem.Game.IsReplay && !GameSystem.Game.IsSingleplayer)
        {
            ProcessTimers();
            SetRemoteTimer();
        }

        return true;
    }

    public void Reverse() { }

    void ProcessTimers()
    {
        foreach (Player p in players)
        {
            if (GameSystem.Game.Turn.CheckEntityOwnedByActivePlayer(p.TimerEntity))
            {
                p.StopTimer();
                ProcessTimer(p);
            }
            else
                p.StartTimer();


            p.ResetTimerSound();
        }

        if (GameSystem.Game.TimerDataReceived)
            UpdateTimerFromRemoteData();

        GameSystem.Game.TimerDataReceived = false;
    }

    void ProcessTimer(Player p)
    {
        if (p.Timer.timerType == TimerType.GameTimer)
            IncrementTimer(p.Timer);
        else
            ResetTurnTime(p.Timer);
    }

    void ResetTurnTime(Timer timer)
    {
        timer.currentTime = timer.startingTime;
        timer.totalTime = timer.startingTime;
    }

    void IncrementTimer(Timer timer)
    {
        if(GameSystem.Game.Turn.GetTurnCount() > 3)
            timer.currentTime += timer.increment;
        timer.totalTime = timer.currentTime;
    }

    void SetRemoteTimer()
    {
        var timerData = new Godot.Collections.Array { GameSystem.Game.Player.Timer.currentTime - GameSystem.Game.Sync.GetDelay() };
        GameSystem.Game.Rpc("ValidateTimer", timerData);
        GameSystem.Game.Sync.TestDelay();
    }

    void UpdateTimerFromRemoteData()
    {
        GameSystem.Game.Enemy.Timer.currentTime = GameSystem.Game.Enemy.ServerCurrentTime;
        GameSystem.Game.Enemy.Timer.totalTime = GameSystem.Game.Enemy.ServerCurrentTime;
        GameSystem.Game.Enemy.Timer.startingTime = GameSystem.Game.Enemy.ServerCurrentTime;
    }
}