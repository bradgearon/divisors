using System.Reflection;
using DG.Tweening;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class PointerExitHandler : MonoBehaviour, 
    IPointerExitHandler,
    IPointerEnterHandler
{
    private GameObject _dragging;
    private DragDropHandler _dragDropHandler;

    void Start()
    {
        _dragDropHandler = GetComponentInChildren<DragDropHandler>();
    }

	// Use this for initialization
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("exited");
        _dragging = eventData.selectedObject;
        eventData.selectedObject = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("entered");
        eventData.selectedObject = _dragging;
        _dragging = null;
    }
}
