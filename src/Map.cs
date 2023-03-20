using System;
using System.Collections.Generic;

public class Map
{
    private readonly Tile[,] map;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public Map(int width, int height)
    {
        map = new Tile[width, height];
        int x, y;

        for (y = 0; y < height; y++)
        {
            for (x = 0; x < width; x++)
            {
                map[x, y] = new Tile();
            }
        }

        Width = width;
        Height = height;
    }

	public Tile GetTile(int x, int y)
	{
		if (!IsInBounds(x, y))
			throw new ArgumentOutOfRangeException("Requested tile outside of bounds of array.");
		else
            return map[x, y];
	}

	public void UpdatePassability(List<Position> positions)
	{
        foreach (Tile tile in map)
            tile.Passable = true;

        foreach(Position position in positions)
            GetTile(position.X, position.Y).Passable = false;
    }

	public void MoveUnit(Tile to, Tile from)
	{
        to.Passable = false;
        from.Passable = true;
	}

    public bool IsPassable(int x, int y)
    {
        return GetTile(x,y).Passable;
    }
    
    public bool IsInBounds(int x, int y)
    {
        if (x < 0
            || y < 0
            || y > Height -1
            || x > Width -1
            )
            return false;

        else return true;
    }
}
