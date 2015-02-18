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

public class TileManager : MonoBehaviour
{
    public Transform TileContainer;
    public ColorSetting[] ColorSettings = new ColorSetting[10];

    public int Min = 2;
    public int Max = 99;

    private Tile[] _tiles = new Tile[30];
    private byte[] _possible;

    void Start()
    {
        if (TileContainer == null) return;

        var imageElements = TileContainer.GetComponentsInChildren<Image>();
        var textElements = TileContainer.GetComponentsInChildren<Text>();

        InitTiles(imageElements, textElements);
        SetLinks();

        var range = Enumerable.Range(Min, Max + 1);
        _possible = BuildFactors(range).ToArray();

        StartCoroutine(RandomizeTiles());
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
                Text = textElements[i],
                Image = imageElements[i]
            };
        }
    }

    /// <summary>
    /// sets up links between tiles
    /// </summary>
    private void SetLinks()
    {
        for (var i = 0; i < _tiles.Length; i++)
        {
            // top
            var top = i - 5;
            if (top >= 0)
            {
                _tiles[i].Top = _tiles[top];
            }

            // bottom
            var bottom = i + 5;
            if (bottom < _tiles.Length)
            {
                _tiles[i].Bottom = _tiles[bottom];
            }

            // if its a new row
            // stop left and right
            // if at the last in the row
            var isFirst = i > 5 && (i - 1) % 5 == 0;
            var isLast = (i + 1) % 5 == 0;

            // left
            var left = i - 1;
            if (!isFirst && left >= 0)
            {
                _tiles[i].Left = _tiles[left];
            }

            // right
            var right = i + 1;
            // todo: limit this to the current row
            if (!isLast && right < _tiles.Length)
            {
                _tiles[i].Right = _tiles[right];
            }
        }
    }



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

    private byte GetNumber()
    {
        var factorIndex = Random.Range(0, _possible.Count() - 1);
        var number = _possible[factorIndex];
        return number;
    }

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

    private void RemoveMatches(Tile tile)
    {
        var leftRank = 0;
        var topRank = 0;

        // move left two
        var leftMost = Move(tile, t => t.Left, 2, out leftRank);

        // move up two
        var topMost = Move(tile, t => t.Top, 2, out topRank);

        // for each that dont match
        var leftMatchCount = 0;
        var topMatchCount = 0;
        do
        {
            leftMatchCount = GetMatches(leftMost, t => t.Right, t => t.Number = GetNumber());
            topMatchCount = GetMatches(topMost, t => t.Bottom, t => t.Number = GetNumber());

        }
        while (leftMatchCount > 1 || topMatchCount > 1);
    }

    private int GetMatches(Tile current, Func<Tile, Tile> direction, Action<Tile> onMatch)
    {
        var factors = current.Number.Factors()
            .ToArray();

        var exclude = new byte[] { 1 };

        var matchCount = 1;
        var moved = 0;

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

            if (matchCount > 2)
            {
                onMatch(current);
            }
            else
            {
                current = Move(current, direction, 1, out moved);
            }

        } while (moved != 0);

        return matchCount;
    }

    private Tile Move(Tile tile, Func<Tile, Tile> func, int distance, out int rank)
    {
        for (rank = 0; rank < distance; rank++)
        {
            var thisTile = func(tile);
            if (thisTile == null)
            {
                break;
            }
            tile = thisTile;
        }
        return tile;
    }

}


