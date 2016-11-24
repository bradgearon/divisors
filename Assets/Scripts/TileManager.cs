using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TileManager : MonoBehaviour
{
    private readonly object _locker = new object();
    private Image[] _imageElements;

    private byte[] _possible;
    private Text[] _textElements;
    private GameObject _tilePrefabInstance;

    private Tile[] _tiles = new Tile[30];
    public ColorSetting[] ColorSettings = new ColorSetting[10];
    public int columns = 5;
    private readonly bool debugPlacement = false;
    public Vector2 DragBounds;
    private readonly byte[] easyFactors = {2, 3, 4, 5, 6, 7, 8, 9};

    private bool easyMode;
    public byte level = 1;
    public int Max = 99;
    public int Min = 2;
    public Transform Panel;
    public int rowPadding = 10;
    public int rows = 6;

    public ScoreManager ScoreManager;

    public Transform TileContainer;
    public GameObject TileEffectPrefab;

    public string tileLayerName = "raylayer";
    public int tilePadding = 10;
    public GameObject TilePrefab;

    public Transform TopTransform;

    public static TileManager Instance { get; set; }

    private void Start()
    {
        easyMode = GameManager.Instance.EasyMode;

        if (GameManager.Instance.SelectedLevel != null)
        {
            var selectedLevel = GameManager.Instance.SelectedLevel;

            Min = selectedLevel.Min;
            Max = selectedLevel.Max;
            level = (byte) selectedLevel.Multiplier;
        }

        _tilePrefabInstance = Instantiate(TilePrefab);
        var tilePrefabTransform = _tilePrefabInstance.GetComponent<RectTransform>();
        Instance = this;

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
        if (rectTransform == null)
        {
            return;
        }

        var size = rectTransform.rect.size;
        var deltaPivot = rectTransform.pivot - pivot;
        var deltaPosition = new Vector2(deltaPivot.x*size.x, deltaPivot.y*size.y);
        rectTransform.pivot = pivot;
        rectTransform.localPosition -= (Vector3) deltaPosition;
    }

    private void CreateTiles(RectTransform tilePrefabTransform)
    {
        var tileCount = rows*columns;

        _tiles = new Tile[tileCount];
        _imageElements = new Image[tileCount];
        _textElements = new Text[tileCount];

        var tileLayer = LayerMask.NameToLayer(tileLayerName);
        var rowPrefab = new GameObject("row");
        var rowPrefabTransform = rowPrefab.AddComponent<RectTransform>();
        rowPrefabTransform.pivot = new Vector2(1, 0);

        rowPrefabTransform.anchorMin = new Vector2(0, 1);
        rowPrefabTransform.anchorMax = new Vector2(1, 1);

        var tileContainerTransform = TileContainer.GetComponent<RectTransform>();
        var tileWidth = (tileContainerTransform.rect.width - columns*tilePadding)/columns;
        Debug.Log("tile width: " + tileWidth);

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

                _imageElements[row*columns + column] = tile.GetComponentInChildren<Image>();
                _textElements[row*columns + column] = tile.GetComponentInChildren<Text>();

                tile.layer = tileLayer;
                tile.name = "Button";
                tile.transform.localScale = Vector3.one;

                var tileTransform = tile.GetComponent<RectTransform>();

                tileTransform.sizeDelta = new Vector2(tileWidth, tileWidth);

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

        var range = Enumerable.Range(Min*level, Max*level + 1);
        _possible = BuildFactors(range).ToArray();

        RandomizeTiles();
        KeepRandomizingTiles();
    }

    private void KeepRandomizingTiles()
    {
        for (var iw = 0; iw < 5; iw++)
        {
            foreach (var tile in _tiles)
            {
                var matches = CheckDirections(tile);
                if (matches.Any(arr => arr.Any()))
                {
                    setTileNumberColor(tile);
                }
            }
        }
    }

    /// <summary>
    ///     first the first tile with matching id
    /// </summary>
    /// <param name="toFind"></param>
    /// <returns></returns>
    public Tile FindMatchingTile(Transform toFind)
    {
        return _tiles.FirstOrDefault(tile =>
                tile.Image.gameObject.GetInstanceID() == toFind.gameObject.GetInstanceID());
    }

    /// <summary>
    ///     finds matches <-- and ^^ two from starting point
    /// </summary>
    /// <param name="current">starting point</param>
    /// <returns>a list of lists... of tiles</returns>
    public IEnumerable<IEnumerable<Tile>> FindMatches(Tile current)
    {
        Debug.Log("enter find matches");

        return CheckDirections(current);
    }

    private IEnumerable<IEnumerable<Tile>> CheckDirections(Tile current)
    {
        var matches = new TileMatch[4];

        matches[0] = GetMatchesNew(current, TileAxis.Horizontal, 1, default(TileMatch));
        matches[1] = GetMatchesNew(current, TileAxis.Horizontal, -1, matches[0]);
        matches[2] = GetMatchesNew(current, TileAxis.Vertical, 1, default(TileMatch));
        matches[3] = GetMatchesNew(current, TileAxis.Vertical, -1, matches[2]);

        return matches
            .Where(arr => arr.Tiles.Length > 2)
            .Select(arr => arr.Tiles.AsEnumerable());
    }

    /// <summary>
    ///     initializes tile set
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

            if (!debugPlacement)
            {
                continue;
            }

            var newText = (Text) Instantiate(_tiles[i].Text, _tiles[i].Text.transform);
            newText.transform.localPosition = new Vector3(5, 1, 0);
            newText.text = string.Format(" {0}", i);
        }
    }


    /// <summary>
    ///     generates the numbers!
    /// </summary>
    public void RandomizeTiles()
    {
        foreach (var tile in _tiles.Where(t => t.Number == 0))
        {
            setTileNumberColor(tile);
        }
    }

    private void setTileNumberColor(Tile tile)
    {
        var color = Random.Range(0, ColorSettings.Length);
        var number = GetNumber();
        tile.Number = number;

        SetTileColor(tile, color);
    }

    private void SetTileColor(Tile tile, int color)
    {
        // using factors 2, 3, 4, 5, 6, 7, 8, 9
        if (easyMode)
        {
            var factors = tile.Number.Factors().OrderByDescending(a => a);
            foreach (var factor in factors)
            {
                var found = false;
                for (var i = 0; i < easyFactors.Length; i++)
                {
                    if (factor != easyFactors[i])
                    {
                        continue;
                    }

                    found = true;
                    color = i;
                    Debug.Log("using color: " + color + " for factor: " + factor + " for number: " + tile.Number);

                    break;
                }

                if (found)
                {
                    break;
                }
            }
        }

        tile.Color = color;
        tile.Image.color = ColorSettings[color].BackColor;
        tile.Text.color = ColorSettings[color].TextColor;
    }

    /// <summary>
    ///     generates a number
    /// </summary>
    /// <returns>the number...</returns>
    private byte GetNumber()
    {
        var factorIndex = Random.Range(0, _possible.Count() - 1);
        var number = _possible[factorIndex];
        return number;
    }

    /// <summary>
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    private IEnumerable<byte> BuildFactors(IEnumerable<int> range)
    {
        var factors = new List<byte>();
        var exclude = new byte[] {1};

        foreach (byte f in range)
        {
            var facts = f.Factors()
                .Except(exclude);

            if ((f < 6) || (facts.Count() > 1))
            {
                factors.Add(f);
            }
        }

        return factors.Distinct();
    }

    private TileMatch GetPreviousMatches(TileMatch match, TileMatch current)
    {
        if ((match.Factors == null) || (match.Factors.Length < 1))
        {
            return match;
        }

        var previousMatching = match.Factors
            .Intersect(current.Factors).ToArray();

        if (previousMatching.Any())
        {
            match.Factors = previousMatching;
            match.Tiles = match.Tiles.Union(current.Tiles).ToArray();
        }

        return match;
    }

    private TileMatch getMatches(TileMatch match, int start, int offset, int max)
    {
        var tiles = new List<Tile>(match.Tiles);
        var factors = match.Factors;

        for (int i = start, moved = 0;
            (i >= 0) && (i < _tiles.Length) && (moved < max);
            i += offset, moved++)
        {
            var tile = _tiles[i];
            var newFactors = factors.Intersect(tile.Number.Factors()).ToArray();
            if (!newFactors.Any())
            {
                break;
            }

            factors = newFactors;
            tiles.Add(tile);
        }

        return new TileMatch
        {
            Tiles = tiles.ToArray(),
            Factors = factors
        };
    }

    private TileMatch GetMatchesNew(Tile start, TileAxis axis, int direction,
        TileMatch previousMatch)
    {
        var axisLength = axis == TileAxis.Horizontal ? 1 : columns;
        var max = Math.Abs(axis == TileAxis.Horizontal
            ? (direction > 0 ? start.Index%columns - columns + 1 : start.Index%columns)
            : _tiles.Length);
        var offset = direction*axisLength;

        var factors = start.Number.Factors().ToArray();
        var tileMatch = new TileMatch
        {
            Tiles = new[] {start},
            Factors = factors
        };

        var newMatch = getMatches(tileMatch, start.Index + offset, offset, max);
        var previous = GetPreviousMatches(previousMatch, tileMatch);

        if (previous.Factors != null)
        {
            var withPrevious = getMatches(previous, start.Index + offset, offset, max);
            if (withPrevious.Tiles.Length > newMatch.Tiles.Length)
            {
                return withPrevious;
            }
        }

        return newMatch;
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

    public YieldInstruction AddStatusTile(IEnumerable<Tile> matches, byte[] values)
    {
        var enumerable = matches as Tile[] ?? matches.ToArray();
        var first = enumerable.FirstOrDefault();
        var last = enumerable.LastOrDefault();
        if ((first == null) || (last == null) || values.Any(a => a == 0))
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
                               .5f*(lastRect.position.y - firstRect.position.y);
        }
        // horizontal matches
        else
        {
            centerPosition.x = firstRect.position.x +
                               .5f*(lastRect.position.x - firstRect.position.x);
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
        SetTileColor(new Tile {Image = image, Text = text}, first.Color);

        GameObject effect = null;
        var instruction = image.transform
            .DOLocalMove(new Vector3(0, 10f), .5f)
            .OnComplete(() =>
            {
                effect = (GameObject) Instantiate(TileEffectPrefab, Panel);
                effect.transform.localScale = Vector3.one;
                effect.transform.localPosition = new Vector3(0, 0, 100);
                effect.GetComponent<ParticleSystem>().DOPlay();
            })
            .WaitForCompletion();

        image.transform
            .DOScale(new Vector3(2f, 2f, 1f), .5f)
            .OnComplete(() =>
            {
                text.DOFade(0f, .25f);
                image.DOFade(0f, .25f);

                ScoreManager.Display(scoreValue);
                ScoreManager.AddScore(scoreValue);
            })
            .WaitForCompletion();
        return instruction;
    }

    public static Vector3 Average(IEnumerable<Vector3> source)
    {
        var parts = source.ToArray();
        return new Vector2(parts.Average(part => part.x),
            parts.Average(part => part.y));
    }

    public YieldInstruction RemovessExistingMatches(Tile[] matches)
    {
        YieldInstruction instruction = null;
        var matchesPositions = matches
            .Select(match => match.Image.transform.position);
        var average = Average(matchesPositions);

        foreach (var tile in matches)
        {
            var tile1 = tile;
            var originalPosition = tile1.Image.transform.position;
            instruction = tile.Image.transform
                .DOMove(average, .5f)
                .OnComplete(() =>
                {
                    tile1.Image.transform.DOScale(Vector2.zero, .5f)
                        .OnComplete(() =>
                        {
                            tile1.Image.transform.localScale = Vector3.one;
                            tile1.Image.transform.position = originalPosition;
                            tile1.Image.DOFade(0f, 0f);
                            tile1.Text.DOFade(0f, 0f);
                        });
                }).WaitForCompletion();
        }

        return instruction;
    }

    public IEnumerable<Tweener> FillTiles()
    {
        lock (_locker)
        {
            var last = _tiles.Last();
            var bottom = last;
            var newArray = new Tile[_tiles.Length];
            var lastBottom = bottom.Index + 1 - columns;


            // for each column (at the bottom)
            while (bottom != null)
            {
                var current = bottom;
                var first = _tiles[bottom.Index - lastBottom];
                var posY = new float[rows];

                var empty = 0;

                var counter = first;
                var ii = 0;

                // count empty tiles
                while (counter != null)
                {
                    posY[ii++] = counter.Image.transform.position.y;

                    var under = counter.Index + columns;
                    counter = under < _tiles.Length ? _tiles[under] : null;
                }

                var spot = rows - 1;
                // for each row
                while (current != null)
                {
                    var index = current.Index;
                    index += columns*empty;

                    // if this one is empty
                    if (current.Number == 0)
                    {
                        index = first.Index;
                        yield return MoveOffAndOnToBoard(current, posY[empty++]);
                        var under = first.Index + columns;
                        first = under < _tiles.Length ? _tiles[under] : null;
                    }
                    else if (empty > 0)
                    {
                        var pos = posY[empty + spot];
                        yield return current.Image.transform.DOMoveY(pos, 1f);
                    }


                    index = Math.Max(0, Math.Min(index, _tiles.Length - 1));

                    newArray[index] = current;

                    var top = current.Index - columns;
                    current = top >= 0 ? _tiles[top] : null;
                    spot--;
                }

                var left = --bottom.Index;

                bottom = left >= lastBottom ? _tiles[left] : null;
            }

            for (var i = 0; i < newArray.Length; i++)
            {
                newArray[i].Index = i;
                _tiles[i] = newArray[i];
            }
        }
    }

    private Tweener MoveOffAndOnToBoard(Tile current, float posY)
    {
        var offBoardY = TopTransform.position.y;
        var image = current.Image;

        current.Image.transform.position = new Vector3(
            image.transform.position.x, offBoardY, image.transform.position.z);

        setTileNumberColor(current);

        return current.Image.transform.DOMoveY(posY, 1f);
    }

    private enum TileAxis
    {
        Horizontal = -1,
        Vertical = 1
    }

    private struct TileMatch
    {
        public Tile[] Tiles { get; set; }
        public byte[] Factors { get; set; }
    }
}