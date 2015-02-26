using System.Collections.Generic;
using UnityEngine;

public interface ITileManager
{
    /// <summary>
    /// first the first tile with matching id
    /// </summary>
    /// <param name="toFind"></param>
    /// <returns></returns>
    Tile FindMatchingTile(Transform toFind);

    /// <summary>
    /// finds matches <-- and ^^ two from starting point
    /// </summary>
    /// <param name="current">starting point</param>
    /// <returns>a list of lists... of tiles</returns>
    IEnumerable<List<Tile>> FindMatches(Tile current);
}