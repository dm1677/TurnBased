using Godot;
using static GameSystem;

public class UILabelTurn : UILabel
{
    public UILabelTurn() : base()
    {
    }

    public override void Update()
    {
        switch (GameSystem.Turn.GetTurnState())
        {
            case TurnState.WaitForInput:
                Label.Text = "My Turn";
                break;
            case TurnState.WaitForEnemyInput:
                Label.Text = "Enemy Turn";
                break;
        }

    }
}