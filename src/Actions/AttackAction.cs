using Godot;
using static GameSystem;

public class AttackAction : Action
{
    public int AttackerID { get; private set; }
    public int DefenderID { get; private set; }
    public bool Killed { get; private set; }
    private int fromX, fromY, damage;


	public AttackAction(int attacker, int defender)
	{
		AttackerID = attacker;
        DefenderID = defender;
        GD.Print($"{this} created: {GetHashCode()}");
        GD.Print($"Attacker: {AttackerID}, Defender: {DefenderID}");
	}

    public override void Execute()
    {
        var attacker = GameSystem.EntityManager.GetEntity(AttackerID);
        var defender = GameSystem.EntityManager.GetEntity(DefenderID);

        var position = GameSystem.EntityManager.GetComponent<Position>(attacker);
        var defenderPosition = GameSystem.EntityManager.GetComponent<Position>(defender);

        Killed = ApplyWeaponDamage(attacker, defender);
        
        if (Killed && IsMoveAttacker(attacker))
        {
            fromX = position.X;
            fromY = position.Y;
            position.X = defenderPosition.X;
            position.Y = defenderPosition.Y;
        }
    }

    //Returns true if the defender's health reached 0
    bool ApplyWeaponDamage(Entity attacker, Entity defender)
    {
        var weapon = GameSystem.EntityManager.GetComponent<Weapon>(attacker);
        var defenderHealth = GameSystem.EntityManager.GetComponent<Health>(defender);

        if (weapon != null && defenderHealth != null)
        {
            if (defenderHealth.CurrentHP < weapon.damage) damage = defenderHealth.CurrentHP;
            else damage = weapon.damage;

            defenderHealth.CurrentHP -= damage;

            if (defenderHealth.CurrentHP <= 0)
                return true;
            else
                return false;
        }

        return false;
    }

    //Returns true if the attacker's movement speed is greater than or equal to its weapon range
    bool IsMoveAttacker(Entity entity)
    {
        var movement = GameSystem.EntityManager.GetComponent<Movement>(entity);
        if (movement == null) return false;

        if (movement.isMoveAttacker) return true;

        return false;
    }

    public override void Undo()
    {
        var attacker = GameSystem.EntityManager.GetEntity(AttackerID);
        Entity defender;

        Position position = GameSystem.EntityManager.GetComponent<Position>(attacker);

        if (Killed)
        {
            defender = GameSystem.EntityManager.RestoreEntity(DefenderID);
            if (IsMoveAttacker(attacker))
            {
                position.X = fromX;
                position.Y = fromY;
            }
        }
        else defender = GameSystem.EntityManager.GetEntity(DefenderID);

        ReverseWeaponDamage(attacker, defender);
    }

    void ReverseWeaponDamage(Entity attacker, Entity defender)
    {
        Weapon weapon = GameSystem.EntityManager.GetComponent<Weapon>(attacker);
        Health defenderHealth = GameSystem.EntityManager.GetComponent<Health>(defender);

        if (weapon != null && defenderHealth != null)
        {
            defenderHealth.CurrentHP += damage;
            if (defenderHealth.CurrentHP > defenderHealth.MaxHP)
                defenderHealth.CurrentHP = defenderHealth.MaxHP;
        }
    }

    public override string[] ReturnData()
    {
        string[] data = new string[3];

        data[0] = GetType().ToString();
        data[1] = AttackerID.ToString();
        data[2] = DefenderID.ToString();

        return data;
    }

}
