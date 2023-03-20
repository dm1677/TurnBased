using Godot;

public class Sound
{
    readonly AudioStreamPlayer audioStream;

    readonly AudioStream turnSound = (AudioStream)GD.Load("res://assets/audio/effects/turn2.wav");
    readonly AudioStream timerWarningSound = (AudioStream)GD.Load("res://assets/audio/effects/timerWarning.wav");

    public enum Effect
    {
        Turn,
        TimerWarning
    }

    public Sound()
    {
        audioStream = new AudioStreamPlayer();
        audioStream.VolumeDb = Options.Volume;
        GameSystem.Game.AddChild(audioStream);
    }

    public void PlaySound(Effect effect)
    {
        AudioStream file = null;
        switch (effect)
        {
            case Effect.Turn:
                file = turnSound;
                break;
            case Effect.TimerWarning:
                file = timerWarningSound;
                break;
        }

        if (file != null)
        {
            audioStream.Stream = file;
            audioStream.Play();
        }
    }

    public void Mute()
    {
        audioStream.VolumeDb = (IsMuted()) ? Options.Volume : -100.0f;
    }

    public bool IsMuted()
    {
        return (audioStream.VolumeDb == -100.0f);
    }
}