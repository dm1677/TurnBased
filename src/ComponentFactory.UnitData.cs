using System;
using System.IO;
using static GameSystem;

public partial class ComponentFactory
{
    public static UnitClass prawn  = new UnitClass() { Name = "Prawn",      Unit = Unit.Prawn,      Sprite = "res://assets/sprites/unit.png" };
    public static UnitClass knight = new UnitClass() { Name = "Knight",     Unit = Unit.Knight,     Sprite = "res://assets/sprites/unit2.png" };
    public static UnitClass king   = new UnitClass() { Name = "King",       Unit = Unit.King,       Sprite = "res://assets/sprites/king.png" };
    public static UnitClass gobbo  = new UnitClass() { Name = "Gobbo",      Unit = Unit.Gobbo,      Sprite = "res://assets/sprites/gobbo_purple.png" };
    public static UnitClass statue = new UnitClass() { Name = "Statue",     Unit = Unit.Building,   Sprite = "res://assets/sprites/newstatue_purple.png" };
    public static UnitClass money  = new UnitClass() { Name = "Money",      Unit = Unit.Resource };

    static readonly string _unitStatsFilePath = AppDomain.CurrentDomain.BaseDirectory + @"\UnitStats.tbb";

    public int UnitCost(Unit unitType)
    {
        switch (unitType)
        {
            case Unit.Building:
                return statue.Cost;

            case Unit.Prawn:
                return prawn.Cost;

            case Unit.Knight:
                return knight.Cost;

            case Unit.Gobbo:
                return gobbo.Cost;
        }

        return -1;
    }

    public UnitClass GetUnitClass(Unit unitType)
    {
        switch (unitType)
        {
            case Unit.Building:
                return statue;

            case Unit.Prawn:
                return prawn;

            case Unit.Knight:
                return knight;

            case Unit.Gobbo:
                return gobbo;

            case Unit.King:
                return king;
        }

        return null;
    }

    static void DataToUnit(BinaryReader reader, UnitClass unit)
    {
        unit.MaxHP          =                   reader.ReadInt32();
        unit.CurrentHP      =                   reader.ReadInt32();
        unit.Regeneration   =                   reader.ReadInt32();
        unit.Speed          =                   reader.ReadInt32();
        unit.IsMoveAttacker =                   reader.ReadBoolean();
        unit.MovementType   =     (MovementType)reader.ReadInt32();
        unit.Damage         =                   reader.ReadInt32();
        unit.Range          =                   reader.ReadInt32();
        unit.AttackType     =       (AttackType)reader.ReadInt32();
        unit.Cost           =                   reader.ReadInt32();
    }

    static void GetValuesFromFile()
    {
        using (var reader = new BinaryReader(File.Open(_unitStatsFilePath, FileMode.Open)))
        {
            while (reader.PeekChar() != -1)
            {
                var s = reader.ReadString();
                switch (s)
                {
                    case "Prawn":
                        DataToUnit(reader, prawn);
                        break;
                    case "King":
                        DataToUnit(reader, king);
                        break;
                    case "Knight":
                        DataToUnit(reader, knight);
                        break;
                    case "Gobbo":
                        DataToUnit(reader, gobbo);
                        break;
                    case "Statue":
                        DataToUnit(reader, statue);
                        break;
                    case "Money":
                        DataToUnit(reader, money);
                        break;
                }
            }
        }
    }
}