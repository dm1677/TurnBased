using Godot;
using System.Collections.Generic;

public class ColourMenu : Control
{
	ColourMod[] colours;

	readonly List<ColourButton> colourButtons = new List<ColourButton>();

	PackedScene colourButton =  (PackedScene)GD.Load("res://src/UI/ColourButton.tscn");
	const int offset = 4, yMargin = 4, buttonSize = 18;

	Color panelColour = new Color(50f/255f,50f/255f,50f/255f);
	ColorRect panel = new ColorRect();

	public ColourMod SelectedColour { get; set; }

	[Signal]
	public delegate void ColourChanged();
	[Signal]
	public delegate void Closed(ColourButton colourButton);

	public override void _Ready()
	{
		InitialiseColours();
		PopulateColours();
		AddChild(panel);
		panel.Color = panelColour;
		panel.RectPosition = new Vector2(RectPosition.x, RectPosition.y + 16);
		panel.RectSize = new Vector2(colours.Length * (buttonSize + 4) + offset, (yMargin * 2) + (buttonSize));
	}

	void InitialiseColours()
	{
		colours = new ColourMod[]
		{
			new ColourMod(-0.5f, 0.2f, 0f),
			new ColourMod(0.0f, -0.28f, -1.0f),
			new ColourMod(-0.5f, 0.6f, -0.5f),
			new ColourMod(1f, 0.5f, -0.5f),
			new ColourMod(0f, 0f, 0f)
		};
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (!Visible) return;
		if (inputEvent is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == (int)ButtonList.Left && mouseEvent.Pressed)
		{
			var mousePos = GetGlobalMousePosition();
			//if (mousePos < panel.GetGlobalRect().Position || mousePos > panel.GetGlobalRect().End)
			if(mousePos.x < panel.RectGlobalPosition.x || mousePos.y < panel.RectGlobalPosition.y || mousePos.x > panel.RectGlobalPosition.x + panel.RectSize.x || mousePos.y > panel.RectGlobalPosition.y + panel.RectSize.y)
				EmitSignal(nameof(Closed), new ColourButton());
		}
	}

	void PopulateColours()
	{
		for (int i = 0; i < colours.Length; i++)
		{
			ColourButton button = (ColourButton)colourButton.Instance();
			panel.AddChild(button);
			button.Initialise();
			button.SetColour(colours[i]);
			button.RectPosition = new Vector2(i * (buttonSize + offset) + offset, yMargin);

			var a = (Button)button.GetNode("Button");
			a.MarginRight = 0;
			a.MarginBottom = 0;

			colourButtons.Add(button);
			button.Connect("Pressed", this, nameof(_On_ColourButtonPressed));

			if (button.Colour == SelectedColour)
				button.SetSelected();
		}
	}

	void _On_ColourButtonPressed(ColourButton colourButton)
	{
		SelectColourButton(colourButton);
		EmitSignal("ColourChanged");
	}

	void SelectColourButton(ColourButton colourButton)
	{
		foreach(ColourButton c in colourButtons)
			c.SetDeselected();
		colourButton.SetSelected();

		SelectedColour = colourButton.Colour;
	}
}
