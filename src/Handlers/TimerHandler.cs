using System.Collections.Generic;
using static GameSystem;

public class TimerHandler : IHandler
{
    readonly HashSet<Player> players = new HashSet<Player>();

    public TimerHandler()
    {
        players.Add(GameSystem.Player);
        players.Add(Enemy);
    }

    public bool Process()
    {
        if (!GameSystem.Game.isReplay && !GameSystem.Game.gameInfo.Singleplayer)
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
            if (GameSystem.Turn.CheckEntityOwnedByActivePlayer(p.TimerEntity))
            {
                p.StopTimer();
                ProcessTimer(p);
            }
            else
                p.StartTimer();


            p.ResetTimerSound();
        }

        if (GameSystem.Game.timerDataReceived)
            UpdateTimerFromRemoteData();

        GameSystem.Game.timerDataReceived = false;
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
        if(GameSystem.Turn.GetTurnCount() > 3)
            timer.currentTime += timer.increment;
        timer.totalTime = timer.currentTime;
    }

    void SetRemoteTimer()
    {
        var timerData = new Godot.Collections.Array { GameSystem.Player.Timer.currentTime - GameSystem.Game.sync.GetDelay() };
        GameSystem.Game.Rpc("ValidateTimer", timerData);
        GameSystem.Game.sync.TestDelay();
    }

    void UpdateTimerFromRemoteData()
    {
        Enemy.Timer.currentTime = Enemy.ServerCurrentTime;
        Enemy.Timer.totalTime = Enemy.ServerCurrentTime;
        Enemy.Timer.startingTime = Enemy.ServerCurrentTime;
    }
}