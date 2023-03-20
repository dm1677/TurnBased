public struct PlayerInfo
{
    public PlayerInfo(string name, int id)
    {
        Name = name;
        ID = id;
    }

    public string Name { get; }
    public int ID { get; }
}