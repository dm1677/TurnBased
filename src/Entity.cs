using System.Collections.Generic;

public class Entity
{
    public int ID { get; }
    public bool QueuedForDeletion { get; set; } = false;

    private readonly IList<Component> components;

    public Entity(int id, IList<Component> componentList)
    {
        ID = id;
        components = componentList;
    }

    public TComponent GetComponent<TComponent>() where TComponent : Component
    {
        foreach (Component component in components)
        {
            if (component is TComponent && !component.Disabled)
                return (TComponent)component;
        }
        return null;
    }
}
