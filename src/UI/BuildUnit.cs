using Godot;
using System.Collections.Generic;

public class BuildUnit : Control
{
	const int _buttonStartX = 20;
	const int _buttonStartY = 12;
	const int _buttonOffsetX = 85;

	public Unit UnitToBuild { get; private set; }    
	public string UnitSprite { get; private set; }
    public bool BuildingUnit { get; set; } = false;

    readonly List<UnitButton> buttons = new List<UnitButton>();

	public override void _Ready()
	{
		CreateButton(ComponentFactory.gobbo);
		CreateButton(ComponentFactory.prawn);
		CreateButton(ComponentFactory.statue);
		CreateButton(ComponentFactory.knight);

		AddButtons();

		foreach(UnitButton button in buttons)
			button.Connect("pressed", this, "ButtonPressed");
	}

	private void AddButtons()
	{
		int x = _buttonStartX, y = _buttonStartY;

		foreach (UnitButton button in buttons)
		{
			var position = new Vector2(x, y);
			button.SetPosition(position);
            button.FocusMode = FocusModeEnum.None;
			x += _buttonOffsetX;

			AddChild(button);
		}
	}

	private Button CreateButton(UnitClass unitClass)
	{
		var button = new UnitButton() { UnitType = unitClass };
		buttons.Add(button);
		return button;
	}

	private void ButtonPressed(Unit unitType, string sprite)
	{
		UnitToBuild = unitType;
		UnitSprite = sprite;
		BuildingUnit = true;
	}
}
