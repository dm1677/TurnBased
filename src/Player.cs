using System.Diagnostics;
using static GameSystem;

public class Player
{
    public int ID { get; private set; }
    public string Name { get; private set; }
    private bool soundPlayed = false;
    public float ServerCurrentTime { get; set; }
    public Entity ResourceEntity { get; set; }
    public GResource Resource { get; set; }
    public Entity TimerEntity { get; set; }
    public Timer Timer { get; private set; }

    readonly Stopwatch stopwatch = new Stopwatch();

    public Player(int networkUniqueID, string name, bool isFirstPlayer)
    {
        ID = SetPlayerID(networkUniqueID, isFirstPlayer);
        Name = name;
    }

    int SetPlayerID(int networkUniqueID, bool isFirstPlayer)
    {
        if (isFirstPlayer)
        {
            if (networkUniqueID == 1)
                return 0;
            else
                return 1;
        }
        else
        {
            if (networkUniqueID == 1)
                return 1;
            else
                return 0;
        }
    }

    public void SetTimer(Entity entity)
    {
        TimerEntity = entity;
        Timer = entity.GetComponent<Timer>();
    }

    public void ProcessTimer()
    {
        if (GameSystem.Game.Turn.MovingPlayerOwnsEntity(TimerEntity) && !GameSystem.Game.ContextManager.Context.GameOver)
        {
            Timer.currentTime = Timer.totalTime - (float)stopwatch.Elapsed.TotalMilliseconds;
            CheckTimeout();
        }
    }

    void CheckTimeout()
    {
        if (Timer.currentTime < 5000 && !soundPlayed)
        {
            GameSystem.Sound.PlaySound(Sound.Effect.TimerWarning);
            soundPlayed = true;
        }

        if (Timer.currentTime <= 0)
        {
            stopwatch.Stop();

            if (ID == GameSystem.Player.ID)
                GameSystem.Game.Rpc("GameResult", Enemy.Name);
        }
    }

    public void StopTimer() { stopwatch.Reset(); }
    public void StartTimer() { stopwatch.Start(); }
    public void ResetTimerSound() { soundPlayed = false; }
    public int GetEnemyID() { return (ID == 0) ? 1 : 0; }
}
