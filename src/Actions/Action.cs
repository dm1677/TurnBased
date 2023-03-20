public abstract class Action
{
    public virtual void Execute() { }
    public virtual void Undo() { }
    public virtual string[] ReturnData()
    {
        string[] data = new string[1];

        data[0] = GetType().ToString();
        return data;
    }
}