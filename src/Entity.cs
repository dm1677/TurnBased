public class Entity
{
    public int ID { get; }
    public bool QueuedForDeletion { get; set; } = false;

    public Entity(int id)
    {
        ID = id;
    }
}
