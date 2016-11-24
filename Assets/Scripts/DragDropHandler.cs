using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDropHandler : MonoBehaviour,
    IBeginDragHandler,
    IEndDragHandler,
    IDragHandler
{
    private GameObject _currentPointer;
    private Vector2 _initialPosition;
    public Vector3 SelectedScale;

    public Transform Panel;

    private GameObject _selectedObject;
    private Transform _initialParent;

    public float RayDistance;
    public LayerMask RayLayerMask;

    private float _dpiScale;
    private float _scaledRayDistance;
    public float radius;
    private float _scaledRadius;
    private Vector2 _delta;
    private bool _inputEnabled = true;
    private Transform _scaler;
    public float threshold = 10;
    private float _scaledThreshold;

    void Start()
    {
        _scaler = GetComponentInParent<CanvasScaler>().transform;

        _scaledRayDistance = _scaler.localScale.x * RayDistance;
        _scaledRadius = _scaler.localScale.x * radius;
        _scaledThreshold = _scaler.localScale.x * threshold;

        Debug.Log("scaled: " + _scaledRayDistance + " ray distance: " + RayDistance);
    }

    private void DisableInput()
    {
        _inputEnabled = false;
    }

    private void EnableInput()
    {
        _inputEnabled = true;
        _selectedObject = null;
        ScoreManager.Instance.CheckOver();
    }

    private IEnumerator CheckMatchedTiles(TileManager tileManager)
    {
        _currentPointer.transform.DOScale(Vector3.one, .25f);

        var dragged = tileManager.FindMatchingTile(
            _selectedObject.gameObject.transform);

        var droppedOn = tileManager.FindMatchingTile(
            _currentPointer.transform);

        _selectedObject.transform.SetParent(_initialParent);

        var draggedTransform = _selectedObject.gameObject.transform;
        var droppedOnTransform = _currentPointer.transform;

        var secondPosition = droppedOnTransform.position;

        draggedTransform
            .DOMove(secondPosition, .5f);

        yield return droppedOnTransform
            .DOMove(_initialPosition, .5f)
            .WaitForCompletion();

        tileManager.ReplaceTile(dragged, droppedOn);

        var matches = tileManager.LongestExclusive(dragged, droppedOn);

        Debug.Log("step taken");
        ScoreManager.Instance.TakeStep();

        foreach (var handleMatch in tileManager.HandleMatchGroups(new [] { matches }))
        {
            yield return handleMatch;
        }

        // it did not find any matches
        tileManager.ReplaceTile(droppedOn, dragged);
        droppedOnTransform
            .DOMove(draggedTransform.position, .5f);

        yield return draggedTransform
            .DOMove(droppedOnTransform.position, .5f)
            .WaitForCompletion();

        EnableInput();

        ScoreManager.Instance.CheckOver();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        eventData.Use();
        if (!_inputEnabled)
        {
            return;
        }

        if (eventData.selectedObject == null)
        {
            return;
        }

        _initialPosition = eventData.selectedObject.transform.position;

        _selectedObject = eventData.selectedObject.gameObject;
        _selectedObject.GetComponent<Collider2D>().enabled = false;

        _initialParent = _selectedObject.transform.parent;
        _selectedObject.transform.SetParent(Panel);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        eventData.Use();
        if (!_inputEnabled || _selectedObject == null)
        {
            return;
        }

        DisableInput();

        _selectedObject.GetComponent<Collider2D>().enabled = true;
        _delta = Vector2.zero;

        if (_currentPointer == null)
        {
            MoveToInitial();
            return;
        }

        var tileManager = TileManager.Instance;

        StartCoroutine(CheckMatchedTiles(tileManager));
    }

    private void MoveToInitial()
    {
        _selectedObject.transform
            .DOMove(_initialPosition, .25f)
            .OnComplete(CompleteMoveToInitial)
            .WaitForCompletion();
    }

    private void CompleteMoveToInitial()
    {
        Debug.Log("complete move to initial");
        _selectedObject.transform.SetParent(_initialParent);
        EnableInput();
    }

    public void OnDrag(PointerEventData eventData)
    {
        eventData.Use();
        if (!_inputEnabled || _selectedObject == null)
        {
            return; 
        }

        _delta += eventData.delta;

        var targetPosition = _initialPosition + Vector2.ClampMagnitude(_delta, _scaledRadius);

        _selectedObject.transform.position = targetPosition;
        var distanceMoved = Vector2.Distance(targetPosition, _initialPosition);
        if (distanceMoved < _scaledThreshold)
        {
            if (_currentPointer != null)
            {
                _currentPointer.transform.DOScale(Vector3.one, .25f);
                _currentPointer = null;
            }
            return;
        }

        var direction = targetPosition - _initialPosition;
        var raycast = Physics2D.Raycast(_initialPosition, direction,
            _scaledRayDistance, RayLayerMask);

        if (raycast.collider == null)
        {
            return;
        }

        var raypointer = raycast.collider.gameObject;
        if (raypointer == null || raypointer == _currentPointer)
        {
            return;
        }

        if (_currentPointer != null)
        {
            _currentPointer.transform.DOScale(Vector3.one, .25f);
        }

        _currentPointer = raypointer;
        _currentPointer.transform.DOScale(SelectedScale, .25f);
    }

    
    
}
