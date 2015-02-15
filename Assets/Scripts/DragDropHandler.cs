using DG.Tweening;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDropHandler : 
    MonoBehaviour,
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
