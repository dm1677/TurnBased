using System.Diagnostics;
using static GameSystem;

public class Player
{
    private readonly int id;
    private readonly string name;
    private bool soundPlayed = false;
    public float ServerCurrentTime { get; set; }
    public Entity ResourceEntity { get; set; }
    public GResource Resource { get; set; }
    public Entity TimerEntity { get; set; }
    public Timer Timer { get; private set; }

    readonly Stopwatch stopwatch = new Stopwatch();

    public Player(int networkUniqueID, string name, bool isFirstPlayer)
    {
        id = SetPlayerID(networkUniqueID, isFirstPlayer);
        this.name = name;
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
        Timer = GameSystem.EntityManager.GetComponent<Timer>(entity);
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

            if (id == GameSystem.Game.Player.GetID())
                GameSystem.Game.Rpc("GameResult", GameSystem.Game.Enemy.GetName());
        }
    }

    public void StopTimer() { stopwatch.Reset(); }
    public void StartTimer() { stopwatch.Start(); }
    public void ResetTimerSound() { soundPlayed = false; }

    public string GetName() { return name; }
    public int GetID() { return id; }
    public int GetEnemyID()
    {
        if (id == 0)
            return 1;
        else
            return 0;
    }
}
