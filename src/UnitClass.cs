using static GameSystem;

public class UnitClass
{   //populated from file
    public int          MaxHP           { get; set; }
    public int          CurrentHP       { get; set; }
    public int          Regeneration    { get; set; }
    public int          Speed           { get; set; }
    public bool         IsMoveAttacker  { get; set; }
    public MovementType MovementType    { get; set; }
    public int          Damage          { get; set; }
    public int          Range           { get; set; }
    public AttackType   AttackType      { get; set; }
    public int          Cost            { get; set; }

    //not yet populated from file
    public string   Name    { get; set; }
    public string   Sprite  { get; set; }
    public Unit     Unit    { get; set; }

    public Entity Create(int x, int y)
    {
        Entity entity = GameSystem.EntityManager.CreateEntity();

        GameSystem.EntityManager.AddComponent(entity, new Name() { name = Name });
        GameSystem.EntityManager.AddComponent(entity, new Sprite() { path = Sprite });
        GameSystem.EntityManager.AddComponent(entity, new Position() { X = x, Y = y });
        GameSystem.EntityManager.AddComponent(entity, new Health(MaxHP, Regeneration) { CurrentHP = CurrentHP, });
        GameSystem.EntityManager.AddComponent(entity, new Weapon() { damage = Damage, range = Range, attackType = AttackType });

        return entity;
    }

    public void AddMovement(Entity entity) { GameSystem.EntityManager.AddComponent(entity, new Movement() { speed = Speed, isMoveAttacker = IsMoveAttacker, movementType = MovementType }); }
}