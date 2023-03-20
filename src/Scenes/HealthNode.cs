using Godot;

class HealthNode : Node2D
{
    Entity entity;
    Health healthComponent;

    readonly Texture healthBlockTexture = (Texture)GD.Load("res://assets/UI/healthblock.png");
    readonly Font healthBlockFont = (Font)GD.Load("res://assets/UI/fonts/main_font_small.tres");

    readonly Vector2 _healthBlockPos = new Vector2(32 - 9, 32 - 9);

    const int tileSize = 32;

    public void Initialise(Entity entity)
    {
        this.entity = entity;
    }

    public override void _Process(float delta)
    {
        if (healthComponent == null)
            healthComponent = GameSystem.EntityManager.GetComponent<Health>(entity.ID);
        Update();
    }

    public override void _Draw()
    {
        DrawHealth();
    }

    void DrawHealth()
    {
        if (healthComponent == null) return;

        if (entity == GameSystem.Input.GetSelection() || Options.HealthBarMode == Render.HealthBarMode.showAll)
            DrawHealth(healthComponent.CurrentHP, healthComponent.MaxHP);
        else if (healthComponent.CurrentHP < healthComponent.MaxHP && Options.HealthBarMode == Render.HealthBarMode.showDamaged)
            DrawHealth(healthComponent.CurrentHP, healthComponent.MaxHP);
    }

    void DrawHealth(int current, int max)
    {
        if (Options.HealthBars) DrawHealthbar(current, max);
        else DrawHealthBlock(current);
    }

    void DrawHealthbar(int current, int max)
    {
        float percentage = ((float)current / (float)max);
        float width = tileSize * percentage;

        int height = (Options.DrawHealthBarsTop) ? 0 : tileSize - 4;

        Rect2 rect = new Rect2(0, height, width, 4);
        Rect2 rect1 = new Rect2(width, height, tileSize * (1 - percentage), 4);

        DrawRect(rect, Options.greenColour, true);
        DrawRect(rect1, Options.redColour, true);
    }

    void DrawHealthBlock(int health)
    {
        var texture = healthBlockTexture;
        var font = healthBlockFont;

        int offset = 9;
        offset = (health < 10) ? 6 : 9;

        if (health >= 10 && health < 20) offset = 8;
        if (health == 1) offset = 4;
        if (health == 11) offset = 6;

        DrawTexture(texture, _healthBlockPos);
        DrawString(font, new Vector2(32 - offset, 32), health.ToString());
    }
}