using Godot;
using System;

public class ChatBox : NinePatchRect
{
	LineEdit input;
	RichTextLabel output;
	string playerName = "localplayer";

	readonly Texture attackTexture = (Texture)GD.Load("res://assets/UI/crossed_swords.png");
	readonly Texture skullTexture = (Texture)GD.Load("res://assets/UI/skull.png");
	readonly Texture plusTexture = (Texture)GD.Load("res://assets/UI/plus.png");
	readonly Texture arrowTexture = (Texture)GD.Load("res://assets/UI/arrow.png");
	readonly Texture swapTexture = (Texture)GD.Load("res://assets/UI/arrow_swap.png");

	[Signal]
	public delegate void ChatSignal(bool b);

	public override void _Ready()
	{
		input = (LineEdit)GetNode("Input");
		output = (RichTextLabel)GetNode("Output");
	}

	public void Initialise(string name, int positionX, int positionY, int width, int height)
	{
		playerName = name;
		SetPosition(new Vector2(positionX, positionY));
		SetSize(new Vector2(width, height));
	}

	public void SetPlayerName(string name)
	{
		playerName = name;
	}

	private void _on_Send_pressed()
	{
		if (input.Text != "" && input.Text.Length < 150)
		{
			//AddChatMessage(playerName, input.Text);
			Rpc(nameof(AddChatMessage), playerName, input.Text);
			input.Text = "";
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent
		&& mouseEvent.IsPressed()
		&& (ButtonList)mouseEvent.ButtonIndex == ButtonList.Left)
			input.ReleaseFocus();
	}

	private void _on_Input_text_entered(String new_text)
	{
		_on_Send_pressed();
	}

	public void ClearChat()
	{
		output.Clear();
	}

	public void LogAttack(Sprite attackerSprite, Sprite defenderSprite, bool killed = false)
	{
		var attackerTexture = (Texture)GD.Load(attackerSprite.path);
		var defenderTexture = (Texture)GD.Load(defenderSprite.path);

		output.AddImage(attackerTexture, 24, 24);
		output.AddImage(attackTexture, 18, 18);
		output.AddImage(defenderTexture, 24, 24);

		if (killed)
		{
			output.AddText("(");
			output.AddImage(skullTexture, 18, 18);
			output.AddText(")");
		}

		output.Newline();
		//output.Pop();
	}

	public void LogCreate(Sprite unitSprite)
	{
		var unitTexture = (Texture)GD.Load(unitSprite.path);

		output.AddImage(plusTexture);
		output.AddImage(unitTexture, 24, 24);

		output.Newline();
	}

	public void LogMove(Sprite unitSprite)
	{
		var unitTexture = (Texture)GD.Load(unitSprite.path);

		output.AddImage(unitTexture, 24, 24);
		output.AddImage(arrowTexture);

		output.Newline();
	}

	public void LogSwap(Sprite firstUnit, Sprite secondUnit)
	{
		var firstUnitTexture = (Texture)GD.Load(firstUnit.path);
		var secondUnitTexture = (Texture)GD.Load(secondUnit.path);

		output.AddImage(firstUnitTexture, 24, 24);
		output.AddImage(swapTexture, 18, 18);
		output.AddImage(secondUnitTexture, 24, 24);

		output.Newline();
	}

	[RemoteSync]
	public void AddChatMessage(string name, string text)
	{
		output.AddText(name + ": " + text + "\n");
	}

	[RemoteSync]
	public void AddMessage(string text)
	{
		output.AddText(text + "\n");
	}

	private void _on_Input_focus_entered()
	{
		EmitSignal(nameof(ChatSignal), false);
	}
	private void _on_Input_focus_exited()
	{
		EmitSignal(nameof(ChatSignal), true);
	}

}
