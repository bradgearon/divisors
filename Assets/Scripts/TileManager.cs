using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DG.Tweening;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TileManager : MonoBehaviour,
    ITileNavigator
{
    public static TileManager Instance
    {
        get { return _instance; }
        set { _instance = value; }
    }

    public Transform TileContainer;
    public ColorSetting[] ColorSettings = new ColorSetting[10];

    public int Min = 2;
    public int Max = 99;

    private static TileManager _instance;

    private Tile[] _tiles = new Tile[30];
    private byte[] _possible;

    void Start()
    {
        _instance = this;
        if (TileContainer == null) return;

        var imageElements = TileContainer.GetComponentsInChildren<Image>();
        var textElements = TileContainer.GetComponentsInChildren<Text>();

        InitTiles(imageElements, textElements);

        var range = Enumerable.Range(Min, Max + 1);
        _possible = BuildFactors(range).ToArray();

        StartCoroutine(RandomizeTiles());
    }

    /// <summary>
    /// first the first tile with matching id
    /// </summary>
    /// <param name="toFind"></param>
    /// <returns></returns>
    public Tile FindMatchingTile(Transform toFind)
    {
        return _tiles.FirstOrDefault(tile =>
            tile.Image.gameObject.GetInstanceID() == toFind.gameObject.GetInstanceID());
    }

    /// <summary>
    /// finds matches <-- and ^^ two from starting point
    /// </summary>
    /// <param name="current">starting point</param>
    /// <returns>a list of lists... of tiles</returns>
    public IEnumerable<List<Tile>> FindMatches(Tile current)
    {
        Debug.Log("enter find matches");
        Tile left;
        Tile top;

        Debug.Log("enter move to top left");
        MoveToTopLeft(current, out left, out top);
        Debug.Log("exit move to top left");

        var matches = new List<Tile>();
        Debug.Log("getMatches left -> right; add");
        var leftMatchCount = GetMatches(left, t => t.Right(), matches.Add);
        if (matches.Count > 2)
        {
            Debug.Log("match count > 2 left -> right");
            yield return matches;
        }

        matches = new List<Tile>();
        Debug.Log("getMatches top -> bottom; add");
        var topMatchCount = GetMatches(top, t => t.Bottom(), matches.Add);
        if (matches.Count > 2)
        {
            Debug.Log("match count > 2 top -> bottom");
            yield return matches;
        }

        Debug.Log("yield return empty");
        yield return null;
    }

    /// <summary>
    /// initializes tile set
    /// </summary>
    /// <param name="imageElements">image elements for tile background</param>
    /// <param name="textElements">text elements for tile foreground</param>
    private void InitTiles(IList<Image> imageElements, IList<Text> textElements)
    {
        for (var i = 0; i < _tiles.Length; i++)
        {
            _tiles[i] = new Tile
            {
                Index = i,
                Text = textElements[i],
                Image = imageElements[i]
            };
        }
    }



    /// <summary>
    /// generates the numbers!
    /// </summary>
    /// <returns>ur mom</returns>
    IEnumerator RandomizeTiles()
    {
        for (var i = 0; i < _tiles.Length; i++)
        {
            var tile = _tiles[i];

            var color = Random.Range(0, ColorSettings.Length);
            var number = GetNumber();

            tile.Image.color = ColorSettings[color].BackColor;
            tile.Text.color = ColorSettings[color].TextColor;
            tile.Number = number;
        }

        for (var iw = 0; iw < 5; iw++)
        {
            foreach (var tile in _tiles)
            {
                RemoveMatches(tile);
            }
        }

        yield return 0;
    }

    /// <summary>
    /// generates a number
    /// </summary>
    /// <returns>the number...</returns>
    private byte GetNumber()
    {
        var factorIndex = Random.Range(0, _possible.Count() - 1);
        var number = _possible[factorIndex];
        return number;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    private IEnumerable<byte> BuildFactors(IEnumerable<int> range)
    {
        var factors = new List<byte>();
        var exclude = new byte[] { 1 };

        foreach (byte f in range)
        {
            var facts = f.Factors()
                .Except(exclude);

            if (f < 6 || facts.Count() > 2)
            {
                factors.Add(f);
            }
        }

        return factors.Distinct();
    }

    private void RemoveMatches(Tile current)
    {
        var leftMost = current;
        var topMost = current;

        MoveToTopLeft(current, out leftMost, out topMost);

        // for each that dont match
        var leftMatchCount = 0;
        var topMatchCount = 0;
        do
        {
            leftMatchCount = GetMatches(leftMost, t => t.Right(), t => t.Number = GetNumber(), 2);
            topMatchCount = GetMatches(topMost, t => t.Bottom(), t => t.Number = GetNumber(), 2);
        }
        while (leftMatchCount > 1 || topMatchCount > 1);
    }

    private void MoveToTopLeft(Tile tile, out Tile left, out Tile top)
    {
        var leftRank = 0;
        var topRank = 0;

        // move left two
        left = Move(tile, t => Left(t.Index), 2, out leftRank);

        // move up two
        top = Move(tile, t => Top(t.Index), 2, out topRank);
    }

    private int GetMatches(Tile current, Func<Tile, Tile> direction, Action<Tile> onMatch,
        int matchCountMax = 1)
    {
        var factors = current.Number.Factors().ToArray();
        var exclude = new byte[] { 1 };

        var matchCount = 1;
        var moved = 0;
        bool resetFactors = false;

        do
        {
            factors = factors
                .Except(exclude)
                .Intersect(current.Number.Factors())
                .ToArray();

            if (factors.Any())
            {
                matchCount++;
            }
            else
            {
                matchCount = 1;
            }

            if (matchCount > matchCountMax)
            {
                onMatch(current);
            }

            current = Move(current, direction, 1, out moved);
        }
        while (moved != 0);

        return matchCount;
    }

    private Tile Move(Tile tile, Func<Tile, Tile> func, int distance, out int rank)
    {
        for (rank = 0; rank < distance; rank++)
        {
            var thisTile = func(tile);
            if (thisTile == null)
            {
                rank = Math.Min(0, rank--);
                break;
            }
            tile = thisTile;
        }
        return tile;
    }

    public void ReplaceTile(Tile sourceTile, Tile destinationTile)
    {
        var sourceIndex = sourceTile.Index;
        var destinationIndex = destinationTile.Index;

        sourceTile.Index = destinationIndex;
        destinationTile.Index = sourceIndex;

        _tiles[sourceIndex] = destinationTile;
        _tiles[destinationIndex] = sourceTile;
    }

    public Tile Left(int i)
    {
        Tile tile = null;
        var isFirst = i > 4 && i % 5 == 0;
        var left = i - 1;

        if (!isFirst && left >= 0)
        {
            tile = _tiles[left];
        }
        return tile;
    }

    public Tile Right(int i)
    {
        Tile tile = null;
        var isLast = (i + 1) % 5 == 0;
        var right = i + 1;
        if (!isLast && right < _tiles.Length)
        {
            tile = _tiles[right];
        }
        return tile;
    }

    public Tile Top(int i)
    {
        Tile tile = null;
        var top = i - 5;
        if (top >= 0)
        {
            tile = _tiles[top];
        }
        return tile;
    }

    public Tile Bottom(int i)
    {
        Tile tile = null;
        var bottom = i + 5;
        if (bottom < _tiles.Length)
        {
            tile = _tiles[bottom];
        }
        return tile;
    }
}


