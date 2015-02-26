using System.Linq;
using System.Security;
using DG.Tweening;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDropHandler : MonoBehaviour, 
    IBeginDragHandler, 
    IDropHandler, 
    IPointerEnterHandler
{
    private readonly ITileManager _tileManager;
    private Text _currentPointer;

    [Construct]
    public DragDropHandler(ITileManager tileManager)
    {
        _tileManager = tileManager;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (_currentPointer == null)
        {
            return;
        }

        var sourceTile = _tileManager.FindMatchingTile(
            eventData.selectedObject.gameObject.transform);
        var destinationTile = _tileManager.FindMatchingTile(
            _currentPointer.transform.parent);
        if (sourceTile.Left != destinationTile && sourceTile.Right != destinationTile &&
            sourceTile.Top != destinationTile && sourceTile.Bottom != destinationTile)
        {
            return;
        }

        var tile = new Tile
        {
            Bottom = destinationTile.Bottom,
            Top = destinationTile.Top,
            Left = destinationTile.Left,
            Right = destinationTile.Right,
            Image = sourceTile.Image,
            Text = sourceTile.Text,
            Number = sourceTile.Number
        };

        var matches = _tileManager.FindMatches(tile).ToArray();
        if (!matches.Any())
        {
            return;
        }

        eventData.selectedObject.gameObject.transform
                .DOMove(_currentPointer.transform.position, .5f);
        _currentPointer.transform.parent
            .DOMove(eventData.selectedObject.gameObject.transform.position, .5f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var button = eventData.selectedObject.GetComponentInChildren<Text>();
        if (button == null) return;

        Debug.Log(button.text);

        eventData.Use();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _currentPointer = eventData.pointerCurrentRaycast
            .gameObject.GetComponent<Text>();

        eventData.Use();
    }
}
