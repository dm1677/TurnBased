using Godot;
using static GameSystem;

public class UILabelHealth : UILabel
{
    public UILabelHealth() : base()
    {
    }

    public override void Update()
    {
        Label.Text = GetHealthAsString();
    }

    string GetHealthAsString()
    {
        var selected = GameSystem.Input.GetSelection();
        if (selected == null) return "";

        Health healthComponent = GameSystem.EntityManager.GetComponent<Health>(selected);
        if (healthComponent == null) return "";

        (int currentHP, int maxHP) = GetHealth(healthComponent);
        return $"{currentHP}/{maxHP}";
    }

    (int currentHP, int maxHP) GetHealth(Health healthComponent)
    {
        int currentHP = healthComponent.CurrentHP;
        int maxHP = healthComponent.MaxHP;

        return (currentHP, maxHP);
                
    }
}