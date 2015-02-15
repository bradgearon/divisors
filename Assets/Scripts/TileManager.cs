using System;
using DG.Tweening;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TileManager : MonoBehaviour
{
    public GameObject TileContainer;
    public Color[] Colors;
    public Color[] TextColors;

    private Image[] _imageElements;
    private Text[] _textElements;
    private int[] _numbers = new int[30];

    void Start()
    {
        if (TileContainer == null) return;

        _imageElements = TileContainer.GetComponentsInChildren<Image>();
        _textElements = TileContainer.GetComponentsInChildren<Text>();

        StartCoroutine(RandomizeTiles());
    }

    IEnumerator RandomizeTiles()
    {
        for (var i = 0; i < _imageElements.Length; i++)
        {
            var image = _imageElements[i];
            var text = _textElements[i];

            var color = Random.Range(0, Colors.Length);
            var number = Random.Range(2, 99);

            image.color = Colors[color];
            text.color = TextColors[color];
            text.text = string.Empty + number;
        }

        yield return 0;
    }



    
}
