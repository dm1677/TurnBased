using Godot;

public class UnitButton : Button
{
    const int _width = 60;
    const int _height = 60;

    public UnitClass UnitType { get; set; }
    private Label infoLabel;

    private Color pressedColour = new Color(0.24f, 1, 0);
    private Color disabledColour = new Color(0.28f, 0.28f, 0.28f);
    private Color hoverColour = new Color(1, 1, 0);
    private Color normalColour = new Color(1, 1, 1);

    private bool unitTypeInitialised = false;

    public override void _Ready()
    {
        SetSize(new Vector2(_width, _height));
    }

    public override void _Process(float delta)
    {
        if (!unitTypeInitialised && UnitType != null)
        {
            CheckCreateLabel();
            SetStyleBoxes();
            unitTypeInitialised = true;
        }

        Disabled = (GameSystem.Player.Resource.Value < UnitType.Cost) ? true : false;

        Update();
    }

    public override void _Pressed()
    {
        EmitSignal("pressed", UnitType.Unit, UnitType.Sprite);
    }

    private void CheckCreateLabel()
    {
        if (infoLabel == null)
        {
            infoLabel = new Label();
            infoLabel.Text = $"{UnitType.Name}: {UnitType.Cost}";
            infoLabel.SetPosition(new Vector2(RectSize.x - _width, RectSize.y + 5));

            AddChild(infoLabel);
        }
    }

    private void SetStyleBoxes()
    {
        var pressed = CreateStyleBox(pressedColour);
        var disabled = CreateStyleBox(disabledColour);
        var hover = CreateStyleBox(hoverColour);
        var normal = CreateStyleBox(normalColour);

        AddStyleboxOverride("normal", normal);
        AddStyleboxOverride("pressed", pressed);
        AddStyleboxOverride("disabled", disabled);
        AddStyleboxOverride("hover", hover);
        AddStyleboxOverride("focus", new StyleBoxEmpty());
    }

    private StyleBoxTexture CreateStyleBox(Color colour)
    {
        var styleBoxTexture = new StyleBoxTexture();
        styleBoxTexture.Texture = (Texture)GD.Load(UnitType.Sprite);
        styleBoxTexture.ModulateColor = colour;

        return styleBoxTexture;
    }
}