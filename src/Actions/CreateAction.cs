using static GameSystem;

public class CreateAction : Action
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int UnitType { get; private set; }
    public int Owner { get; private set; } //owner: 0 = host, 1 = client
    public int ResourceEntityID { get; private set; }
    public Entity CreatedEntity { get; private set; } = null;


    public CreateAction(int x, int y, int unitType, int owner, int resourceEntityID)
	    {
		    X = x;
		    Y = y;
            UnitType = unitType;
            Owner = owner;
            ResourceEntityID = resourceEntityID;
	    }

	public override void Execute()
	{
        if (CreatedEntity == null)
        {
            CreatedEntity = ComponentFactory.Instance().CreateUnit(X, Y, (Unit)UnitType);

            var component = new Owner() { ownedBy = (User)Owner };
            GameSystem.EntityManager.AddComponent(CreatedEntity, component);
        }
        else
        {
            GameSystem.EntityManager.RestoreEntity(CreatedEntity.ID);
        }
        
        ReduceResources();        
    }

    public override void Undo()
    {
        ReverseReduceResources();
        if (Input.GetSelection() == CreatedEntity)
            Input.SetNullSelection();
        GameSystem.EntityManager.DeleteEntity(CreatedEntity);
    }

    public override string[] ReturnData()
    {
        string[] data = new string[6];

        data[0] = GetType().ToString();
        data[1] = X.ToString();
        data[2] = Y.ToString();
        data[3] = UnitType.ToString();
        data[4] = Owner.ToString();
        data[5] = ResourceEntityID.ToString();

        return data;
    }

    void ReduceResources()
    {
        var owningPlayer = (User)Owner;
        var entityList = GameSystem.EntityManager.GetEntityList().Keys;

        foreach (Entity entity in entityList)
        {
            var resource = GameSystem.EntityManager.GetComponent<GResource>(entity);
            var owner = GameSystem.EntityManager.GetComponent<Owner>(entity);

            if (owner != null && resource != null && owner.ownedBy == owningPlayer)
            {
                var cost = ResourceHandler.CostToBuildUnit((Unit)UnitType, resource);
                if (cost != -1)
                    resource.Value -= cost;
            }
        }
    }

    void ReverseReduceResources()
    {
        var owningPlayer = (User)Owner;
        var entityList = GameSystem.EntityManager.GetEntityList().Keys;

        foreach (Entity entity in entityList)
        {
            GResource resource = GameSystem.EntityManager.GetComponent<GResource>(entity);
            Owner owner = GameSystem.EntityManager.GetComponent<Owner>(entity);

            if (owner != null && resource != null && owner.ownedBy == owningPlayer)
            {
                var cost = ComponentFactory.Instance().UnitCost((Unit)UnitType);
                if (cost != -1)
                    resource.Value += cost;
            }
        }
    }
}
