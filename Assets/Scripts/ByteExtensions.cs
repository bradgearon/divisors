using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class ByteExtensions
{
    public static IEnumerable<byte> Factors(this byte x)
    {
        var max = (byte)x / 2;
        for (byte i = 1; i <= max; i++)
        {
            if (0 == (x % i))
            {
                yield return i;
            }
        }

        yield return x;
    }

    public static Tile Left(this Tile tile)
    {
        return TileManager.Instance.Left(tile.Index);
    }

    public static Tile Right(this Tile tile)
    {
        return TileManager.Instance.Right(tile.Index);
    }

    public static Tile Bottom(this Tile tile)
    {
        return TileManager.Instance.Bottom(tile.Index);
    }

    public static Tile Top(this Tile tile)
    {
        return TileManager.Instance.Top(tile.Index);
    }
}
