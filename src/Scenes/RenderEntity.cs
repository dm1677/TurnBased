using Godot;
using static GameSystem;

class RenderEntity : Godot.Sprite
{
    public Entity entity { get; private set; }

    Position positionComponent;
    Owner ownerComponent;
    Sprite spriteComponent;

    HealthNode healthNode;

    readonly Texture selectedTexture         = (Texture)GD.Load("res://assets/UI/selected.png");
    readonly Texture selectedEnemyTexture    = (Texture)GD.Load("res://assets/UI/selectedEnemy.png");
    readonly Texture selectedNeutralTexture  = (Texture)GD.Load("res://assets/UI/selectedNeutral.png");

    readonly Shader unitShader = (Shader) GD.Load("res://assets/unit_shader.shader");

    const int offset = 16;
    const int tileSize = 32;

    bool performChecks = true;

    public void Initialise(Entity entity)
    {
        this.entity = entity;
        Offset = new Vector2(offset, offset);

        SetShader();

        healthNode = new HealthNode();
        healthNode.Initialise(entity);
        AddChild(healthNode);
    }

    public override void _Process(float delta)
    {
        if(performChecks) CheckSetComponents();

        SetShaderValues();

        if ((positionComponent != null) && (Position.x != positionComponent.X * tileSize || Position.y != positionComponent.Y * tileSize))
            Position = new Vector2(positionComponent.X * tileSize, positionComponent.Y * tileSize);

        Update();
    }

    void SetShaderValues()
    {
        if (Material != null && ownerComponent != null)
        {
            var r = (ownerComponent.ownedBy == (User)GameSystem.Player.ID) ? Options.FriendlyColour.R : Options.EnemyColour.R;
            var g = (ownerComponent.ownedBy == (User)GameSystem.Player.ID) ? Options.FriendlyColour.G : Options.EnemyColour.G;
            var b = (ownerComponent.ownedBy == (User)GameSystem.Player.ID) ? Options.FriendlyColour.B : Options.EnemyColour.B;

            Material.Set("shader_param/red", r);
            Material.Set("shader_param/green", g);
            Material.Set("shader_param/blue", b);
        }
        else if (Options.ColourWholeUnit && Material == null && ownerComponent != null)
            SetShader();
    }

    void SetShader()
    {
        Material = new ShaderMaterial() { Shader = unitShader };
    }

    void CheckSetComponents()
    {
        if (ownerComponent == null)
            ownerComponent = GameSystem.EntityManager.GetComponent<Owner>(entity);
        if(spriteComponent == null)
            spriteComponent = GameSystem.EntityManager.GetComponent<Sprite>(entity);
        if(positionComponent == null)
            positionComponent = GameSystem.EntityManager.GetComponent<Position>(entity);
        if (spriteComponent != null && Texture == null)
            Texture = (Texture)GD.Load(spriteComponent.path);

        if (ownerComponent != null && spriteComponent != null && positionComponent != null)
            performChecks = false;
    }

    public override void _Draw()
    {
        DrawSelected();
    }

    void DrawSelected()
    {
        if (positionComponent == null || ownerComponent == null) return;

        var owner = ownerComponent.ownedBy;

        var friendly = (User)GameSystem.Player.ID;

        Texture selected = selectedNeutralTexture;
        Color drawColour = Options.neutralColour;

        if (owner == friendly)
        {
            selected = selectedTexture;
            drawColour = Options.FriendlyColour;
        }
        else if (owner != friendly)
        {
            selected = selectedEnemyTexture;
            drawColour = Options.EnemyColour;
        }

        if (owner == User.Neutral)
        {
            selected = selectedNeutralTexture;
            drawColour = Options.neutralColour;
        }

        SelfModulate = Options.ColourWholeUnit ? drawColour : Render.NullModulate;

        if (GameSystem.Input.GetSelection() == entity)
            DrawTexture(selected, new Vector2(0 , 0));
    }
}