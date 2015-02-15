using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class DragDropHandler : MonoBehaviour,
    IDragHandler,
    IDropHandler
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnDrop(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
