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
    private Text _currentPointer;

    public void OnDrop(PointerEventData eventData)
    {
        if (_currentPointer == null)
        {
            return;
        }
        var tileManager = TileManager.Instance;

        Debug.Log("find matching first tile");
        var dragged = tileManager.FindMatchingTile(
            eventData.selectedObject.gameObject.transform);
        Debug.Log("find matching dropped on to tile");
        var droppedOn = tileManager.FindMatchingTile(
            _currentPointer.transform.parent);

        if (dragged.Left() != droppedOn 
            && dragged.Right() != droppedOn 
            && dragged.Top() != droppedOn 
            && dragged.Bottom() != droppedOn)
        {
            return;
        }

        var draggedTransform = eventData.selectedObject.gameObject.transform;
        var droppedOnTransform = _currentPointer.transform.parent;

        SwitchPositions(draggedTransform, droppedOnTransform);
        tileManager.ReplaceTile(dragged, droppedOn);

        var matches = tileManager.FindMatches(dragged).ToList();
        matches.AddRange(tileManager.FindMatches(droppedOn));

        if (matches.Any(m => m != null))
        {
            return;
        }

        SwitchPositions(droppedOnTransform, draggedTransform);
        tileManager.ReplaceTile(dragged, droppedOn);
        
    }

    private void SwitchPositions(Transform first, Transform second)
    {
        var secondPosition = second.position;
        var firstPosition = first.position;
        first.DOMove(secondPosition, .5f);
        second.DOMove(firstPosition, .5f);
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
