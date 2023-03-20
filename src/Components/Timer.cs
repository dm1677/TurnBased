public class Timer : Component
{
    public TimerType timerType;
    public float totalTime, startingTime, currentTime, increment;

    public Timer(TimerType timerType, int totalTime, int increment)
    {
        this.timerType = timerType;
        this.totalTime = totalTime * 1000;
        this.startingTime = totalTime * 1000;
        this.increment = increment * 1000;

        currentTime = totalTime * 1000;
    }
}