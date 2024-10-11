using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EventHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler, IPointerEnterHandler, IPointerExitHandler/*, IDragHandler, IEndDragHandler, IBeginDragHandler*/
{
    public Action<PointerEventData, Transform> OnClickHandler;
    public Action<PointerEventData, Transform> OnDownHandler;
    public Action<PointerEventData, Transform> OnMoveHandler;
    public Action<PointerEventData, Transform> OnUpHandler;

    public Action<PointerEventData, Transform> OnEnterHandler;
    public Action<PointerEventData, Transform> OnExitHandler;

    //public Action<PointerEventData, Transform> OnBeginDragHandler;
    //public Action<PointerEventData, Transform> OnDragHandler;
    //public Action<PointerEventData, Transform> OnEndDragHandler;

    //public void OnBeginDrag(PointerEventData eventData)
    //{
    //    OnBeginDragHandler?.Invoke(eventData, transform);
    //}

    //public void OnDrag(PointerEventData eventData)
    //{
    //    OnDragHandler?.Invoke(eventData, transform);
    //}

    //public void OnEndDrag(PointerEventData eventData)
    //{
    //    OnEndDragHandler?.Invoke(eventData, transform);
    //}

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickHandler?.Invoke(eventData, transform);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDownHandler?.Invoke(eventData, transform);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnEnterHandler?.Invoke(eventData, transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnExitHandler?.Invoke(eventData, transform);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        OnMoveHandler?.Invoke(eventData, transform);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnUpHandler?.Invoke(eventData, transform);
    }
}
