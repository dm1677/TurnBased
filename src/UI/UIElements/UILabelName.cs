using Godot;
using static GameSystem;

public class UILabelName : UILabel
{
    public UILabelName() : base()
    {
    }

    public override void Update()
    {
        var selected = GameSystem.Input.GetSelection();

        if (selected == null)
            Label.Text = "";
        else
        {
            Name nameComponent = GameSystem.EntityManager.GetComponent<Name>(selected);
            if (nameComponent != null)
                Label.Text = nameComponent.name;
        }
        
    }
}