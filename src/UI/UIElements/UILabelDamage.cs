using System;
using Godot;
using static GameSystem;

public class UILabelDamage : UILabel
{
	Weapon weapon;

	public UILabelDamage() : base()
	{
	}

	public override void Update()
	{
		Label.Text = GetDamageAsString();
	}

	string GetDamageAsString()
	{
		Entity selected = GameSystem.Input.GetSelection();
		if (selected == null) return "";

        Weapon weaponComponent = selected.GetComponent<Weapon>();
		if (weaponComponent == null) return "";

		int damage = GetDamage(weaponComponent);
		return $"Damage: {damage}";
	}

	int GetDamage(Weapon weaponComponent)
	{
		return weaponComponent.damage;
	}
}
