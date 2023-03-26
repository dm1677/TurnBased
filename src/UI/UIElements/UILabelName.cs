using Godot;
using static GameSystem;

public class UILabelName : UILabel
{
    public UILabelName() : base()
    {
    }

    public override void Update()
    {
        Entity selected = GameSystem.Input.GetSelection();

        if (selected == null)
            Label.Text = "";
        else
        {
            Name nameComponent = selected.GetComponent<Name>();
            if (nameComponent != null)
                Label.Text = nameComponent.name;
        }
        
    }
}