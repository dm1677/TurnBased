using Godot;

public class ColourButton : Control
{
	public ColourMod Colour { get; private set; }

	readonly Color purple = new Color(229f/255f, 80f/255f, 228/255f);
	readonly Color borderDeselected = new Color(0, 0, 0);
	readonly Color borderSelected = new Color(1, 1, 1);

	ColorRect colourRect;
	ColorRect border;

	public bool Selected { get; private set; }

	[Signal]
	public delegate void Pressed(ColourButton colourButton);

	public void Initialise()
	{
		border = (ColorRect)GetNode("Border");
		colourRect = (ColorRect)GetNode("Colour");

		Colour = new ColourMod(0, 0, 0);
	}

	public void SetColour(ColourMod colourMod)
	{
		Colour = colourMod;
		colourRect.Color = new Color(purple.r + Colour.R, purple.g + Colour.G, purple.b + Colour.B);
	}

	private void _on_Button_pressed()
	{
		EmitSignal("Pressed", this);
	}

	public void SetSelected()
	{
		Selected = true;
		border.Color = borderSelected;            
	}

	public void SetDeselected()
	{
		Selected = false;
		border.Color = borderDeselected;
	}
}
