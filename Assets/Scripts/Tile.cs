using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Tile : IComparable<Tile>
{
    public Kind kind;
    public Suit? suit;
    public int? value;
    public Direction? direction;
    public bool isVirtual;
    public TileMono tileMono;

    public int Id;

    public Tile(int? v = null, Suit? s = null, Direction? dir = null, bool virt = true)
    {
        SetValues(v, s, dir);
    }

    public Tile(int id, int? v = null, Suit? s = null, Direction? dir = null, bool virt = true)
    {
        Id = id;
        SetValues(v, s, dir);
    }

    public Tile(TileMono tm, int id, int? v = null, Suit? s = null, Direction? dir = null, bool virt = false)
    {
        tileMono = tm;
        tileMono.tile = this;
        Id = id;
        isVirtual = virt;

        SetValues(v, s, dir);

        tileMono.Init();
    }

    void SetValues(int? v = null, Suit? s = null, Direction? dir = null)
    {
        // Numbers and Dragons
        if (v != null)
        {
            kind = v == 0 ? Kind.dragon : Kind.number;
            value = v;
            suit = s;
        }
        // Flowers and Winds
        else if (dir != null)
        {
            kind = Kind.flowerwind;
            direction = dir;
        }
        // Jokers
        else kind = Kind.joker;
    }

    public static bool IsValidTileId(int tileId) => tileId >= 0 && tileId < 152;
    public bool IsJoker() => kind == Kind.joker;
    public static bool IsJoker(int tileId) => tileId >= 144;

    public override int GetHashCode()
    {
        return HashCode.Combine(kind, suit, value, direction);
    }

    public override bool Equals(object obj)
    {
        // base checks
        if (this == obj) return true;
        if (obj == null) return false;
        if (obj.GetType() != GetType()) return false;

        Tile that = (Tile)obj;
        // value checks
        if (this.kind != that.kind) return false;
        if (this.suit != that.suit) return false;
        if (this.value != that.value) return false;
        if (this.direction != that.direction) return false;

        // if everything above passed, then return true
        return true;
    }

    public int CompareTo(Tile that)
    {
        if (this.Equals(that)) return 0;
        return Id.CompareTo(that.Id);
    }

    public override string ToString()
    {
        switch (kind)
        {
            case Kind.flowerwind:
                return direction.ToString();
            case Kind.number:
                return $"{value} {suit}";
            case Kind.dragon:
                switch (suit)
                {
                    case Suit.bam:
                        return "Green";
                    case Suit.crak:
                        return "Red";
                    case Suit.dot:
                        return "Soap";
                    default:
                        throw new Exception("invalid dragon suit");
                }
            case Kind.joker:
                return "Joker";
            default:
                throw new Exception("invalid kind");
        }
    }

    public static List<Tile> GenerateTiles()
    {
        int tileId = 0;
        List<Tile> tiles = new();

        // Number dragons
        Suit[] suits = (Suit[])Enum.GetValues(typeof(Suit));

        foreach (Suit suit in suits)
        {
            for (int num = 0; num < 10; num++)
            {
                for (int i = 0; i < 4; i++)
                {
                    tiles.Add(new(tileId++, num, suit));
                }
            }
        }

        // Flower Winds
        Direction[] directions = (Direction[])Enum.GetValues(typeof(Direction));

        foreach (Direction dir in directions)
        {
            for (int id = 0; id < 4; id++)
            {
                tiles.Add(new(tileId++, null, null, dir));
            }
        }

        for (int id = 0; id < 4; id++)
        {
            tiles.Add(new(tileId++, null, null, Direction.flower));
        }

        // Jokers
        for (int id = 0; id < 8; id++)
        {
            tiles.Add(new(tileId++));
        }

        return tiles;
    }
}
