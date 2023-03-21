using Godot;
using static GameSystem;

public class UILabelResource : UILabel
{
    GResource playerResource = null;

    public UILabelResource() : base()
    {
        GetPlayerResource();
    }

    public override void Update()
    {
        if(playerResource !=null)
            Label.Text = $"{playerResource.Name}: {playerResource.Value}";
    }

    void GetPlayerResource()
    {
        var currentPlayer = GameSystem.Player.ID;

        var entityList = GameSystem.EntityManager.GetEntityList().Keys;
        foreach (Entity entity in entityList)
        {
            GResource resourceComponent = GameSystem.EntityManager.GetComponent<GResource>(entity);
            if (resourceComponent == null) continue;
            Owner o = GameSystem.EntityManager.GetComponent<Owner>(entity);
            User owner = o.ownedBy;

            if (owner == (User)currentPlayer)
                playerResource = resourceComponent;
        }

    }
}