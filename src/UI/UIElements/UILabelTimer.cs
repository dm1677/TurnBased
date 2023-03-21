using System;
using Godot;

public class UILabelTimer : UILabel
{
	private readonly Timer timer;
    private readonly string playerName;
    private TimeSpan timeSpan;
    private readonly DynamicFont font = (DynamicFont)GD.Load("res://assets/UI/fonts/timer_font.tres");

	public UILabelTimer(Player player) : base()
	{
		timer = player.Timer;
        playerName = player.Name;

        Label.AddFontOverride("font", font);
	}

	public override void Update()
	{
        timeSpan = TimeSpan.FromMilliseconds(timer.currentTime);
        //Label.Text = $"{playerName}\n{timeSpan.ToString("mm':'ss':'fff")}";
        //Label.Text = timeSpan.ToString("mm':'ss':'fff");
        Label.Text = timeSpan.ToString("mm':'ss");
    }
}
