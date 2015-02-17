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
    
    void Start()
    {
        if (TileContainer == null) return;

        var imageElements = TileContainer.GetComponentsInChildren<Image>();
        var textElements = TileContainer.GetComponentsInChildren<Text>();

        InitTiles(imageElements, textElements);
        SetLinks();

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

            // left
            var left = i - 1;
            if (left >= 0)
            {
                _tiles[i].Left = _tiles[left];
            }

            // right
            var right = i + 1;
            if (right < _tiles.Length)
            {
                _tiles[i].Right = _tiles[right];
            }
        }
    }

    

    IEnumerator RandomizeTiles()
    {
        var range = Enumerable.Range(Min, Max + 1);
        var factors = BuildFactors(range).ToArray();

        for (var i = 0; i < _tiles.Length; i++)
        {
            var tile = _tiles[i];

            var color = Random.Range(0, ColorSettings.Length);
            var factorIndex = Random.Range(0, factors.Count() - 1);
            var number = factors[factorIndex];

            tile.Image.color = ColorSettings[color].BackColor;
            tile.Text.color = ColorSettings[color].TextColor;
            tile.Number = number;
            
            var score = CalculateScore(tile);
        }

        yield return 0;
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

    private byte CalculateScore(Tile tile)
    {
        var score = (byte) 100;

        var leftRank = 0;
        var topRank = 0;

        // move left two
        var leftMost = Move(tile, t => t.Left, 2, out leftRank);

        // move up two
        var topMost = Move(tile, t => t.Top, 2, out topRank);
        
        // for each that dont match
        // subtract 100 - (distance * 25)

        // check left to right for matches
        var rank = 0;
        while (rank < 5)
        {
            var result = 0;
            if (result == 0)
            {
                break;
            }
            rank ++;
        }

        // check top to bottom for matches
        
        
        
        
        return score;
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

    private byte GetMatches(Func<Tile> direction, IEnumerable<byte> factors)
    {
        var tile = direction();
        if (tile == null)
        {
            return 0;
        }

        return tile.Number.Factors()
            .DefaultIfEmpty()
            .Intersect(factors)
            .OrderBy(factor => factor)
            .FirstOrDefault();
    }
}


