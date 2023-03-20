using Godot;

public class UILabel
{
    public Label Label { get; protected set; }

    public UILabel()
    {
        Label = new Label();
    }
    
    public virtual void Update() { }
}