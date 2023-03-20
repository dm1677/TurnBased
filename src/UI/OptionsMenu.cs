using Godot;

public class OptionsMenu : Control
{
	HSlider volumeBar;
	OptionButton healthbarMode;
	OptionButton healthbarStyle;
	CheckBox colourWholeUnit;
	ColourButton colourButtonFriendly, colourButtonEnemy;
	ColourMenu colourMenuFriendly, colourMenuEnemy;

	public bool ToggledOn { get; private set; }

	[Signal]
	public delegate void Toggled();

	public override void _Ready()
	{
		ShowBehindParent = false;
		ToggledOn = false;

		volumeBar = (HSlider)GetNode("Options/Volume");
		healthbarMode = (OptionButton)GetNode("Options/HealthbarMode");
		healthbarStyle = (OptionButton)GetNode("Options/HealthbarStyle");
		colourWholeUnit = (CheckBox)GetNode("Options/ColourWholeUnit");

		InitialiseColourButtons();

		LoadValues();
	}

	void InitialiseColourButtons()
	{
		//friendly
		colourButtonFriendly = (ColourButton)GetNode("Options/FriendlyColour");
		colourMenuFriendly = (ColourMenu)GetNode("Options/FriendlyColour/ColourMenu");

		colourButtonFriendly.Connect("Pressed", this, nameof(_on_FriendlyColour_Pressed));
		colourButtonFriendly.Initialise();
		colourButtonFriendly.SetColour(Options.FriendlyColour);

		colourMenuFriendly.SelectedColour = Options.FriendlyColour;
		colourMenuFriendly.Connect("Closed", this, nameof(_on_FriendlyColour_Pressed));
		colourMenuFriendly.Connect("ColourChanged", this, nameof(_on_FriendlyColour_Changed));

		//enemy
		colourButtonEnemy = (ColourButton)GetNode("Options/EnemyColour");
		colourMenuEnemy = (ColourMenu)GetNode("Options/EnemyColour/ColourMenu");

		colourButtonEnemy.Connect("Pressed", this, nameof(_on_EnemyColour_Pressed));
		colourButtonEnemy.Initialise();
		colourButtonEnemy.SetColour(Options.EnemyColour);

		colourMenuEnemy.SelectedColour = Options.EnemyColour;
		colourMenuEnemy.Connect("Closed", this, nameof(_on_EnemyColour_Pressed));
		colourMenuEnemy.Connect("ColourChanged", this, nameof(_on_EnemyColour_Changed));
	}

	void LoadValues()
	{
		volumeBar.Value = Options.Volume;
		healthbarMode.Selected = (int)Options.HealthBarMode;
		healthbarStyle.Selected = (Options.HealthBars == false) ? 0 : 1;
		colourWholeUnit.Pressed = Options.ColourWholeUnit;
	}

	private void _on_Save_pressed()
	{
		Options.Volume = (float)volumeBar.Value;
		Options.HealthBarMode = (Render.HealthBarMode)healthbarMode.Selected;
		Options.HealthBars = healthbarStyle.Selected != 0;
		Options.ColourWholeUnit = colourWholeUnit.Pressed;

		Options.FriendlyColour = colourMenuFriendly.SelectedColour;
		Options.EnemyColour = colourMenuEnemy.SelectedColour;

		Options.WriteToFile();

		ToggleMenu();
	}

	private void _on_Cancel_pressed()
	{
		ToggleMenu();
	}

	public void ToggleMenu()
	{
		if (Visible)
		{
			Hide();            
			if (GameSystem.Input != null)
			{
				GameSystem.Input.AcceptKeyboardInput(true);
				GameSystem.Input.AcceptMouseInput(true);
			}
		}
		else
		{
			Show();
			if (GameSystem.Input != null)
			{
				GameSystem.Input.AcceptKeyboardInput(false);
				GameSystem.Input.AcceptMouseInput(false);
			}
		}

		ToggledOn = !ToggledOn;
		EmitSignal("Toggled");
	}

	public void _on_FriendlyColour_Pressed(ColourButton colourButton)
	{
		colourMenuFriendly.Visible = !colourMenuFriendly.Visible;

		if (colourMenuFriendly.Visible && colourMenuEnemy.Visible)
			colourMenuEnemy.Visible = false;

		if (colourMenuFriendly.SelectedColour != Options.EnemyColour)
			colourButtonFriendly.SetColour(colourMenuFriendly.SelectedColour);
	}

	public void _on_EnemyColour_Pressed(ColourButton colourButton)
	{
		colourMenuEnemy.Visible = !colourMenuEnemy.Visible;

		if (colourMenuFriendly.Visible && colourMenuEnemy.Visible)
			colourMenuFriendly.Visible = false;

		if (colourMenuEnemy.SelectedColour != Options.FriendlyColour)
			colourButtonEnemy.SetColour(colourMenuEnemy.SelectedColour);       
	}
	
	public void _on_FriendlyColour_Changed()
	{
		if (colourMenuFriendly.SelectedColour != Options.EnemyColour)
			colourButtonFriendly.SetColour(colourMenuFriendly.SelectedColour);
	}

	public void _on_EnemyColour_Changed()
	{
		if (colourMenuEnemy.SelectedColour != Options.FriendlyColour)
			colourButtonEnemy.SetColour(colourMenuEnemy.SelectedColour);
	}
}
