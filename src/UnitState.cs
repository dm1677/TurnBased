using System;
using System.Collections;

public struct UnitState
{
    public Unit         UnitType    { get; private set; }
    public UnitClass    UnitClass   { get; private set; }
    public User         Owner       { get; private set; }
    public int          Health      { get; private set; }
    public int          X           { get; private set; }
    public int          Y           { get; private set; }
    
    public UnitState(Unit unitType, User owner, int health, int x, int y)
    {
        UnitType = unitType;
        UnitClass = ComponentFactory.Instance().GetUnitClass(unitType);
        Owner = owner;
        Health = health;
        X = x;
        Y = y;
    }

    public static bool operator ==(UnitState unit1, UnitState unit2)
    {
        return unit1.GetHashCode() == unit2.GetHashCode();
    }

    public static bool operator !=(UnitState unit1, UnitState unit2)
    {
        return unit1.GetHashCode() != unit2.GetHashCode();
    }

    public void IncrementHealth()
    {
        Health++;
    }

    public UnitState Clone()
    {
        return new UnitState(UnitType, Owner, Health, X, Y);
    }

    public UnitState Clone(int newHealth)
    {
        return new UnitState(UnitType, Owner, newHealth, X, Y);
    }

    public UnitState Clone(int newX, int newY)
    {
        return new UnitState(UnitType, Owner, Health, newX, newY);
    }
}

public struct UnitStateStruct
{
    private byte coords; //4 bits for x, 4 bits for y
    private byte health;
    private byte type_owner; // 3 bits for type, 1 bit for owner

    public UnitStateStruct(byte coords, byte health, byte type_owner)
    {
        this.coords = coords;
        this.health = health;
        this.type_owner = type_owner;
    }

    public UnitStateStruct(int x, int y, int health, Unit unitType, User owner)
    {
        this.health = ConvertToByte(HealthToBitArray(health));
        type_owner = ConvertToByte(OwnerAndUnitTypeToBitArray(unitType, owner));
        coords = ConvertToByte(CoordsToBitArray(x, y));
    }

    static BitArray OwnerAndUnitTypeToBitArray(Unit unitType, User owner)
    {
        var unitTypeBytes = BitConverter.GetBytes((int)unitType);
        var ownerBytes = new byte[1] { 0 };

        if (owner == User.Player) ownerBytes[0] = 1;

        var unitTypeArray = new BitArray(unitTypeBytes);
        var ownerArray = new BitArray(ownerBytes);

        var finalArray = new BitArray(8);

        for (int i = 0; i < 3; i++)
            finalArray[i] = unitTypeArray[i];

        finalArray[4] = ownerArray[0];

        return finalArray;
    }

    static BitArray HealthToBitArray(int health)
    {
        var healthBytes = BitConverter.GetBytes(health);
        var healthArray = new BitArray(healthBytes);

        var finalArray = new BitArray(8);

        for (int i = 0; i < 4; i++)
            finalArray[i] = healthArray[i];

        return finalArray;
    }

    static BitArray CoordsToBitArray(int x, int y)
    {
        if (x > 15 || y > 15) throw new ArgumentOutOfRangeException();

        var xBytes = BitConverter.GetBytes(x);
        var yBytes = BitConverter.GetBytes(y);

        var xArr = new BitArray(xBytes);
        var yArr = new BitArray(yBytes);

        var finalArray = new BitArray(8);

        for (int i = 0; i < 4; i++)
        {
            finalArray[i] = xArr[i];
            finalArray[i + 4] = yArr[i];
        }

        return finalArray;
    }

    static byte ConvertToByte(BitArray bits)
    {
        if (bits.Count != 8)
            throw new ArgumentException("Invalid bit count");

        byte[] bytes = new byte[1];
        bits.CopyTo(bytes, 0);

        return bytes[0];
    }
}

public struct UnitStateStructBool
{
    public bool[] coords { get; private set; }
    public bool[] health { get; private set; }
    public bool[] unitType { get; private set; }
    public bool owner { get; private set; }

    public UnitStateStructBool(int x, int y, int health, Unit unitType, User owner)
    {
        if (x < 0 || y < 0 || x > 15 || y > 15) throw new ArgumentOutOfRangeException();

        coords = new bool[8];

        var xArray = IntToBinary(x);
        var yArray = IntToBinary(y);

        xArray.CopyTo(coords, 0);
        yArray.CopyTo(coords, 4);

        this.health = BitVectorToBoolArray(health);

        if (owner == User.Player) this.owner = true;
        else this.owner = false;

        this.unitType = new bool[3];
        this.unitType = UnitType(unitType);
    }

    public UnitStateStructBool(bool[] coords, bool[] health, bool[] unitType, bool owner)
    {
        this.coords = coords;
        this.health = health;
        this.unitType = unitType;
        this.owner = owner;
    }

    public bool GetOwner()
    {
        return owner;
    }

    static bool[] BitVectorToBoolArray(int i)
    {
        var vector = new System.Collections.Specialized.BitVector32(i);
        bool[] array = new bool[8];
        
        for(int j = 7, k = 0; j > 0; j--, k++)
            array[k] = vector[j];

        return array;
    }

    public static bool[] UnitType(Unit unitType)
    {
        bool[] array = new bool[3];

        int i = (int)unitType;

        for (int j = 0; j < 3; j++)
            array[j] = false;

        switch (i)
        {
            case 0:
                return array;
            case 1:
                array[0] = true;
                return array;
            case 2:
                array[1] = true;
                return array;
            case 3:
                array[0] = true;
                array[1] = true;
                return array;
            case 4:
                array[2] = true;
                return array;
            case 5:
                array[0] = true;
                array[2] = true;
                return array;
            case 6:
                array[1] = true;
                array[2] = true;
                return array;
            case 7:
                array[0] = true;
                array[1] = true;
                array[2] = true;
                return array;
            default:
                return array;
        }
    }


    static bool[] IntToBinary(int i)
    {
        bool[] array = new bool[4];

        for (int j = 0; j < 4; j++)
            array[j] = false;

        switch (i)
        {
            case 0:
                return array;
            case 1:
                array[0] = true;
                return array;
            case 2:
                array[1] = true;
                return array;
            case 3:
                array[0] = true;
                array[1] = true;
                return array;
            case 4:
                array[2] = true;
                return array;
            case 5:
                array[0] = true;
                array[2] = true;
                return array;
            case 6:
                array[1] = true;
                array[2] = true;
                return array;
            case 7:
                array[0] = true;
                array[1] = true;
                array[2] = true;
                return array;
            case 8:
                array[3] = true;
                return array;
            case 9:
                array[0] = true;
                array[3] = true;
                return array;
            case 10:
                array[1] = true;
                array[3] = true;
                return array;
            case 11:
                array[0] = true;
                array[1] = true;
                array[3] = true;
                return array;
            case 12:
                array[2] = true;
                array[3] = true;
                return array;
            case 13:
                array[0] = true;
                array[2] = true;
                array[3] = true;
                return array;
            case 14:
                array[1] = true;
                array[2] = true;
                array[3] = true;
                return array;
            case 15:
                array[0] = true;
                array[1] = true;
                array[2] = true;
                array[3] = true;
                return array;
            default:
                return array;
        }
    }

    public UnitStateStructBool Clone()
    {
        return new UnitStateStructBool(coords, health, unitType, owner);
    }

    public UnitStateStructBool Clone(int x, int y)
    {
        var c = new bool[8];

        var xArray = IntToBinary(x);
        var yArray = IntToBinary(y);

        xArray.CopyTo(c, 0);
        yArray.CopyTo(c, 4);

        return new UnitStateStructBool(c, health, unitType, owner);
    }

}