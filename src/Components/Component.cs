public abstract class Component
{
    public virtual Entity Parent { get; set; }
    public virtual bool Disabled { get; set; } = false;
}