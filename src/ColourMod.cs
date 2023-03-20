using System.Collections.Generic;

public struct ColourMod
{
    public readonly float R, G, B;

    public ColourMod(float r, float g, float b)
    {
        R = r;
        G = g;
        B = b;
    }

    public static implicit operator Godot.Color(ColourMod colourMod)
    {
        return new Godot.Color(0.898f + colourMod.R,
                               0.314f + colourMod.G,
                               0.894f + colourMod.B);
    }

    public static bool operator ==(ColourMod first, ColourMod second)
    {
        return first.R == second.R && first.G == second.G && first.B == second.B;
    }

    public static bool operator !=(ColourMod first, ColourMod second)
    {
        return (first.R != second.R || first.G != second.G || first.B != second.B);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is ColourMod)) return false;
        ColourMod other = (ColourMod)obj;
        return this == other;
    }
}