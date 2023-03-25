using Godot;

public static class Logging
{
    public const bool LOGGING_ENABLED = true;

    public static void Log(string str)
    {
        if (!LOGGING_ENABLED) { return; }
        GD.Print(str);
    }
}