using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDropHandler : MonoBehaviour,
    IBeginDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IDragHandler
{
    private Text _currentPointer;
    private Vector2 _initialPosition;

    public Transform Panel;

    private GameObject _selectedObject;
    private Transform _initialParent;
    private float _angle;

    private IEnumerator CheckMatchedTiles(PointerEventData eventData, TileManager tileManager)
    {
        Debug.Log("find matching first tile");
        var dragged = tileManager.FindMatchingTile(
            _selectedObject.gameObject.transform);
        Debug.Log("find matching dropped on to tile");
        var droppedOn = tileManager.FindMatchingTile(
            _currentPointer.transform.parent);

        if (dragged.Left() != droppedOn
            && dragged.Right() != droppedOn
            && dragged.Top() != droppedOn
            && dragged.Bottom() != droppedOn)
        {
            MoveToInitial();
            yield break;
        }

        _selectedObject.transform.SetParent(_initialParent);
        var draggedTransform = _selectedObject.gameObject.transform;
        var droppedOnTransform = _currentPointer.transform.parent;

        var secondPosition = droppedOnTransform.position;

        draggedTransform.DOMove(secondPosition, .5f);

        yield return droppedOnTransform.DOMove(_initialPosition, .5f).WaitForCompletion();

        tileManager.ReplaceTile(dragged, droppedOn);

        var matches = tileManager.FindMatches(dragged).ToList();
        // matches.AddRange(tileManager.FindMatches(droppedOn));

        if (matches.Any(m => m != null))
        {
            foreach (var tiles in matches)
            {
                if (tiles != null)
                {
                    var ee = HandleTiles(tiles, tileManager);
                    while (ee.MoveNext())
                    {
                        yield return ee.Current;
                    }
                }
            }

            yield break;
        }

        tileManager.ReplaceTile(droppedOn, dragged);
        droppedOnTransform.DOMove(draggedTransform.position, .5f);
        yield return draggedTransform.DOMove(droppedOnTransform.position, .5f).WaitForCompletion();

    }

    private IEnumerator HandleTiles(List<Tile> tiles, TileManager tileManager)
    {
        yield return tileManager.RemovessExistingMatches(tiles);
        var ee = tileManager.AddStatusTile(tiles);
        while (ee.MoveNext())
        {
            yield return ee.Current;
        }
        ee = tileManager.FillTiles();
        while (ee.MoveNext())
        {
            yield return ee.Current;
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.selectedObject == null)
        {
            return;
        }

        _initialPosition = eventData.selectedObject.transform.position;
        _angle = 0f;

        _selectedObject = eventData.selectedObject.gameObject;
        _initialParent = _selectedObject.transform.parent;
        _selectedObject.transform.SetParent(Panel);

        eventData.Use();
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("drop");

        if (_currentPointer == null)
        {
            MoveToInitial();
            return;
        }
        var tileManager = TileManager.Instance;

        StartCoroutine(CheckMatchedTiles(eventData, tileManager));
    }

    private void MoveToInitial()
    {
        _selectedObject.transform.DOMove(_initialPosition, 1f, true)
            .OnComplete(CompleteMoveToInitial);
    }

    private void CompleteMoveToInitial()
    {
        _selectedObject.transform.SetParent(_initialParent);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _currentPointer = eventData.pointerCurrentRaycast
            .gameObject.GetComponent<Text>();

        eventData.Use();
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        _currentPointer = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_selectedObject == null)
        {
            return;
        }

        var targetPosition = Vector2.Min(eventData.position, _initialPosition + TileManager.Instance.DragBounds);
        targetPosition = Vector2.Max(targetPosition, _initialPosition - TileManager.Instance.DragBounds);

        _selectedObject.transform.position = Vector2.Lerp(_selectedObject.transform.position,
            targetPosition, Time.deltaTime * 15f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnDrop(eventData);
    }
}
