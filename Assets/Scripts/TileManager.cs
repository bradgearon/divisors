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

    public ScoreManager ScoreManager;
    public Vector2 DragBounds;
    public GameObject TilePrefab;
    public GameObject TileEffectPrefab;

    public Transform TileContainer;
    public ColorSetting[] ColorSettings = new ColorSetting[10];

    public string tileLayerName = "raylayer";
    public int Min = 2;
    public int Max = 99;
    public int rows = 6;
    public int columns = 5;
    public int rowPadding = 10;
    public int tilePadding = 10;
    public byte level = 1;

    private readonly object _locker = new object();
    private static TileManager _instance;

    private Tile[] _tiles = new Tile[30];
    private Image[] _imageElements;
    private Text[] _textElements;

    private byte[] _possible;
    private GameObject _tilePrefabInstance;

    public Transform TopTransform;
    public Transform Panel;

    void Start()
    {
        _tilePrefabInstance = Instantiate(TilePrefab);
        var tilePrefabTransform = _tilePrefabInstance.GetComponent<RectTransform>();
        _instance = this;

        if (TileContainer == null)
        {
            return;
        }

        if (TileContainer.childCount == 0)
        {
            CreateTiles(tilePrefabTransform);
        }
        else
        {
            _tiles = new Tile[30];
            _imageElements = TileContainer.GetComponentsInChildren<Image>();
            _textElements = TileContainer.GetComponentsInChildren<Text>();
        }

        RestartLevel();
    }

    public static void SetPivot(RectTransform rectTransform, Vector2 pivot)
    {
        if (rectTransform == null) return;

        var size = rectTransform.rect.size;
        var deltaPivot = rectTransform.pivot - pivot;
        var deltaPosition = new Vector2(deltaPivot.x * size.x, deltaPivot.y * size.y);
        rectTransform.pivot = pivot;
        rectTransform.localPosition -= (Vector3) deltaPosition;
    }

    private void CreateTiles(RectTransform tilePrefabTransform)
    {
        var tileCount = rows * columns;

        _tiles = new Tile[tileCount];
        _imageElements = new Image[tileCount];
        _textElements = new Text[tileCount];

        var tileLayer = LayerMask.NameToLayer(tileLayerName);
        var rowPrefab = new GameObject("row");
        var rowPrefabTransform = rowPrefab.AddComponent<RectTransform>();
        rowPrefabTransform.pivot = new Vector2(1, 0);

        rowPrefabTransform.anchorMin = new Vector2(0, 1);
        rowPrefabTransform.anchorMax = new Vector2(1, 1);

        for (var row = 0; row < rows; row++)
        {
            // create the row
            var rowContainer = (GameObject) Instantiate(rowPrefab, TileContainer, false);

            rowContainer.name = "row";
            rowContainer.transform.localScale = Vector3.one;

            var rowTransform = rowContainer.GetComponent<RectTransform>();

            // set padding
            var rowHeight = tilePrefabTransform.sizeDelta.y + rowPadding;

            rowTransform.anchoredPosition = new Vector2(0, (row + 1)*-rowHeight - rowPadding);
            rowTransform.sizeDelta = new Vector2(0, rowHeight);

            // fill it with tiles
            for (var column = 0; column < columns; column++)
            {
                var tile = (GameObject) Instantiate(_tilePrefabInstance, rowContainer.transform, false);

                tile.transform.SetSiblingIndex(column);
                _imageElements[row * columns + column] = tile.GetComponentInChildren<Image>();
                _textElements[row * columns + column] = tile.GetComponentInChildren<Text>();

                tile.layer = tileLayer;
                tile.name = "Button";
                tile.transform.localScale = Vector3.one;

                var tileTransform = tile.GetComponent<RectTransform>();
                tileTransform.pivot = new Vector2(0, 0);

                tileTransform.anchorMin = new Vector2(0, 0);
                tileTransform.anchorMax = new Vector2(0, 0);

                var leftPadding = tilePadding*column + tilePadding;
                tileTransform.anchoredPosition = new Vector2(
                    column*tilePrefabTransform.sizeDelta.x + leftPadding, tilePadding);

                SetPivot(tileTransform, new Vector2(0.5f, 0.5f));
            }
        }
    }

    private void RestartLevel()
    {
        InitTiles(_imageElements, _textElements);

        var range = Enumerable.Range(Min * level, (Max * level) + 1);
        _possible = BuildFactors(range).ToArray();

        RandomizeTiles();
        MakeShitHarder();
    }

    private void MakeShitHarder()
    {
        for (var iw = 0; iw < 5; iw++)
        {
            foreach (var tile in _tiles)
            {
                var matches = RemoveMatches(tile).ToArray();
                if (matches.Length > 0)
                {
                    tile.Number = GetNumber();
                }
            }
        }
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
    public IEnumerable<IEnumerable<Tile>> FindMatches(Tile current)
    {
        Tile left;
        Tile top;

        Debug.Log("enter find matches");
        MoveToTopLeft(current, out left, out top);

        return GetMatches(left, t => t.Right()).ToArray()
            .Union(GetMatches(top, t => t.Bottom()).ToArray());
    }

    /// <summary>
    /// initializes tile set
    /// </summary>
    /// <param name="imageElements">image elements for tile background</param>
    /// <param name="textElements">text elements for tile foreground</param>
    private void InitTiles(Image[] imageElements, Text[] textElements)
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
    ///
    public void RandomizeTiles()
    {
        foreach (var tile in _tiles.Where(t => t.Number == 0))
        {
            var color = Random.Range(0, ColorSettings.Length);
            SetTileColor(tile, color);

            var number = GetNumber();
            tile.Number = number;
        }
    }

    private void SetTileColor(Tile tile, int color)
    {
        tile.Color = color;
        tile.Image.color = ColorSettings[color].BackColor;
        tile.Text.color = ColorSettings[color].TextColor;
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

            if (f < 6 || facts.Count() > 1)
            {
                factors.Add(f);
            }
        }

        return factors.Distinct();
    }

    private IEnumerable<IEnumerable<Tile>> RemoveMatches(Tile current)
    {
        Tile leftMost;
        Tile topMost;

        MoveToTopLeft(current, out leftMost, out topMost);

        return GetMatches(leftMost, t => t.Right(), 1)
            .Union(GetMatches(topMost, t => t.Bottom(), 1))
            .ToArray();
    }

    private void MoveToTopLeft(Tile tile, out Tile left, out Tile top, int maxX = 5, int maxY = 6)
    {
        var leftRank = 0;
        var topRank = 0;

        left = Move(tile, t => t.Left(), maxX, out leftRank);
        top = Move(tile, t => t.Top(), maxY, out topRank);
    }

    private IEnumerable<IEnumerable<Tile>> GetMatches(
        Tile current, Func<Tile, Tile> direction, int matchCountMax = 2)
    {
        var exclude = new byte[] { 1 };
        var factors = new byte[0];
        var matches = new List<Tile>() { current };

        var matchCount = 1;
        var moved = 0;

        do
        {
            var currentFactors = current.Number.Factors()
                .Except(exclude)
                .ToArray();

            factors = factors
                .Except(exclude)
                .Intersect(currentFactors)
                .ToArray();

            if (factors.Any())
            {
                // add to the buffer
                matches.Add(current);
                matchCount++;
            }
            else
            {
                if (matchCount > matchCountMax)
                {
                    // if you have a buffer yield it
                    yield return matches;
                }
                // reset
                matches = new List<Tile>() { current };
                factors = currentFactors;
                matchCount = 1;
            }

            current = Move(current, direction, 1, out moved);
        }
        while (moved != 0);

        if (matchCount > matchCountMax)
        {
            yield return matches;
        }
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
        lock (_locker)
        {
            var sourceIndex = sourceTile.Index;
            var destinationIndex = destinationTile.Index;

            sourceTile.Index = destinationIndex;
            _tiles.SetValue(sourceTile, destinationIndex);

            destinationTile.Index = sourceIndex;
            _tiles.SetValue(destinationTile, sourceIndex);
        }
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
        if (!isLast && right <= _tiles.Length)
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

    public YieldInstruction AddStatusTile(IEnumerable<Tile> matches, byte[] values)
    {
        var enumerable = matches as Tile[] ?? matches.ToArray();
        var first = enumerable.FirstOrDefault();
        var last = enumerable.LastOrDefault();
        if (first == null || last == null || values.Any(a => a == 0))
        {
            return null;
        }

        var firstRect = first.Image.transform;
        var lastRect = last.Image.transform;

        var centerPosition = new Vector3(lastRect.position.x, lastRect.position.y, 100);
        // vertical matches
        if (Mathf.Approximately(firstRect.position.x, lastRect.position.x))
        {
            centerPosition.y = firstRect.position.y +
                (.5f * (lastRect.position.y - firstRect.position.y));
        }
        // horizontal matches
        else
        {
            centerPosition.x = firstRect.position.x +
                (.5f * (lastRect.position.x - firstRect.position.x));
        }

        var child = _tilePrefabInstance;
        child.transform.SetParent(Panel);
        child.transform.localScale = Vector3.one;
        child.transform.position = centerPosition;

        var image = child.GetComponent<Image>();
        var text = child.GetComponentInChildren<Text>();

        IEnumerable<byte> factors = null;

        foreach (var value in values)
        {
            Debug.Log("found match: " + value);
            var valueFactors = value.Factors();
            factors = (factors ?? valueFactors).Intersect(valueFactors);
        }

        factors = values.Aggregate(factors,
            (current, match) => current.Intersect(match.Factors()))
            .OrderByDescending(factor => factor);

        var scoreValue = factors
            .FirstOrDefault(f => f > 1);

        text.text = scoreValue + string.Empty;
        SetTileColor(new Tile { Image = image, Text = text }, first.Color);

        GameObject effect = null;
        image.transform
            .DOLocalMove(new Vector3(0, 10f), .5f)
            .OnComplete(() =>
            {
                effect = (GameObject)Instantiate(TileEffectPrefab, Panel);
                effect.transform.localScale = Vector3.one;
                effect.transform.localPosition = new Vector3(0, 0, 100);
                effect.GetComponent<ParticleSystem>().DOPlay();
            })
            .WaitForCompletion();

        var instruction = image.transform
            .DOScale(new Vector3(2f, 2f, 1f), .75f)
            .OnComplete(() =>
            {
                text.DOFade(0f, .5f);
                image.DOFade(0f, .5f);
            })
            .WaitForCompletion();

        ScoreManager.AddScore(scoreValue);

        return instruction;
    }

    public YieldInstruction RemovessExistingMatches(Tile[] matches)
    {
        YieldInstruction instruction = null;
        foreach (var tile in matches)
        {
            tile.Image.DOFade(0f, .75f);
            instruction = tile.Text.DOFade(0f, .75f)
                .WaitForCompletion();
        }

        return instruction;
    }

    public Tweener FillTiles()
    {
        lock (_locker)
        {
            Tweener instruction = null;
            var last = _tiles.Last();
            var bottom = last;
            var newArray = new Tile[30];

            // for each column (at the bottom)
            while (bottom != null)
            {
                var current = bottom;
                var first = _tiles[bottom.Index - 25];
                var posY = new float[6];

                var empty = 0;

                var counter = first;
                var ii = 0;
                // count empty tiles
                while (counter != null)
                {
                    posY[ii++] = counter.Image.transform.position.y;
                    counter = counter.Bottom();
                }

                var spot = 5;
                // for each row
                while (current != null)
                {
                    var index = current.Index;
                    index += (5 * empty);

                    // if this one is empty
                    if (current.Number == 0)
                    {
                        index = first.Index;
                        var pos = posY[empty++];
                        instruction = MoveOffAndOnToBoard(current, posY[0], empty, pos);

                        first = first.Bottom();
                    }
                    else if (empty > 0)
                    {
                        var pos = posY[empty + spot];
                        instruction = current.Image.transform.DOMoveY(pos, 1f);
                    }


                    index = Math.Max(0, Math.Min(index, 29));
                    Debug.Log("tile: " + current.Index + " now: " + index);

                    newArray[index] = current;
                    current = current.Top();
                    spot--;
                }

                bottom = bottom.Left();
            }

            for (var i = 0; i < newArray.Length; i++)
            {
                newArray[i].Index = i;
                _tiles[i] = newArray[i];

            }

            return instruction;
        }
    }

    private Tweener MoveOffAndOnToBoard(Tile current,
        float posFirstY, int emptySpots, float posY,
        Action after = null)
    {
        var color = current.Image.color;
        var textColor = current.Text.color;
        var offBoardY = TopTransform.position.y;
        var image = current.Image;
        var text = current.Text;

        current.Image.transform.position = new Vector3(
            image.transform.position.x, offBoardY, image.transform.position.z);
        current.Number = GetNumber();
        image.color = new Color(color.r, color.g, color.b, 1f);
        text.color = new Color(textColor.r, textColor.g, textColor.b, 1f);

        return current.Image.transform.DOMoveY(posY, 1f);
    }

    public void OnRestartClick()
    {
        RestartLevel();
    }

}


