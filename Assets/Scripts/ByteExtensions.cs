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
}
