using Godot;
using System.Collections.Generic;

public class LobbyPlayers : VBoxContainer
{
    GameSetup gameSetup;
    Label playerName, enemyName;
    readonly List<Label> labels = new List<Label>();

    public override void _Ready()
    {
        gameSetup = (GameSetup)GetParent().GetParent().GetParent();

        gameSetup.Connect("PlayerInfoChanged", this, nameof(RefreshPlayerList));

        playerName = CreateLabel();
        enemyName = CreateLabel();
    }

    private void RefreshPlayerList()
    {
        ClearAllLabels();

        int i = 0;
        foreach(PlayerInfo playerInfo in gameSetup.PlayerInfo)
        {
            labels[i].Text = playerInfo.Name;
            i++;
        }
    }

    private void ClearAllLabels()
    {
        foreach (Label label in labels)
        {
            label.Text = "";
        }
    }

    private void RemovePlayer()
    {

    }

    public Label CreateLabel(string text = "")
    {
        var label = new Label();
        label.Text = text;

        labels.Add(label);

        AddChild(label);
        return label;
    }
}