using System.IO;

public class Options
{
    public static float Volume { get; set; } = -26f;

    public static readonly Godot.Color greenColour    =   new Godot.Color(0, 256, 0);
    public static readonly Godot.Color redColour      =   new Godot.Color(256, 0, 0);
    public static readonly Godot.Color neutralColour  =   new Godot.Color(1, 1, 1);

    public static ColourMod FriendlyColour { get; set; } = new ColourMod(-0.5f, 0.2f, 0f);
    public static ColourMod EnemyColour { get; set; } = new ColourMod(0.0f, -0.28f, -1.0f);

    public static Render.HealthBarMode HealthBarMode { get; set; } = Render.HealthBarMode.showDamaged;
    public static bool HealthBars { get; set; } = false;
    public static bool DrawHealthBarsTop { get; set; } = true;
    public static bool ColourWholeUnit { get; set; } = false;

    public static Godot.KeyList HotkeyGobbo { get; set; }  = Godot.KeyList.G;
    public static Godot.KeyList HotkeyPrawn { get; set; } = Godot.KeyList.P;
    public static Godot.KeyList HotkeyStatue { get; set; } = Godot.KeyList.B;
    public static Godot.KeyList HotkeyKnight { get; set; } = Godot.KeyList.K;

    public static void WriteToFile()
    {
        using (var writer = new BinaryWriter(File.Open("options.dat", FileMode.Create)))
        {
            writer.Write("settings");
            writer.Write(Volume);
            writer.Write((int)HealthBarMode);
            writer.Write(HealthBars);
            writer.Write(ColourWholeUnit);
            writer.Write("friendly_colour");
            writer.Write(FriendlyColour.R);
            writer.Write(FriendlyColour.G);
            writer.Write(FriendlyColour.B);
            writer.Write("enemy_colour");
            writer.Write(EnemyColour.R);
            writer.Write(EnemyColour.G);
            writer.Write(EnemyColour.B);
        }
    }

    public static void ReadFromFile()
    {
        if (!File.Exists("options.dat")) return;
        using (var reader = new BinaryReader(File.Open("options.dat", FileMode.Open)))
        {
            while (reader.PeekChar() != -1)
            {
                var s = reader.ReadString();

                float r, g, b;

                switch (s)
                {
                    case "settings":
                        Volume = reader.ReadSingle();
                        HealthBarMode = (Render.HealthBarMode)reader.ReadInt32();
                        HealthBars = reader.ReadBoolean();
                        ColourWholeUnit = reader.ReadBoolean();
                        break;
                    case "friendly_colour":
                        r = reader.ReadSingle();
                        g = reader.ReadSingle();
                        b = reader.ReadSingle();
                        FriendlyColour = new ColourMod(r, g, b);
                        break;
                    case "enemy_colour":
                        r = reader.ReadSingle();
                        g = reader.ReadSingle();
                        b = reader.ReadSingle();
                        EnemyColour = new ColourMod(r, g, b);
                        break;
                }
            }
        }
    }
}