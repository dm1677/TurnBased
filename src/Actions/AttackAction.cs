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
        Logging.Log($"{this} created: {GetHashCode()}");
        Logging.Log($"Attacker: {AttackerID}, Defender: {DefenderID}");
	}

    public override void Execute()
    {
        var attacker = GameSystem.EntityManager.GetEntity(AttackerID);
        var defender = GameSystem.EntityManager.GetEntity(DefenderID);

        var position = attacker.GetComponent<Position>();
        var defenderPosition = defender.GetComponent<Position>();

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
        var weapon = attacker.GetComponent<Weapon>();
        var defenderHealth = defender.GetComponent<Health>();

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
        var movement = entity.GetComponent<Movement>();
        if (movement == null) return false;

        if (movement.isMoveAttacker) return true;

        return false;
    }

    public override void Undo()
    {
        var attacker = GameSystem.EntityManager.GetEntity(AttackerID);
        Entity defender;

        Position position = attacker.GetComponent<Position>();

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
        Weapon weapon = attacker.GetComponent<Weapon>();
        Health defenderHealth = defender.GetComponent<Health>();

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
