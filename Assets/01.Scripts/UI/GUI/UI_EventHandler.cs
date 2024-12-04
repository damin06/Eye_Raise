using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI 이벤트를 처리하는 핸들러 클래스입니다.
/// 포인터 관련 이벤트(클릭, 드래그 등)를 처리합니다.
/// </summary>
public class UI_EventHandler : MonoBehaviour,
    IPointerClickHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerMoveHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    #region Event Actions
    public Action<PointerEventData, Transform> OnClickHandler { get; set; }
    public Action<PointerEventData, Transform> OnDownHandler { get; set; }
    public Action<PointerEventData, Transform> OnMoveHandler { get; set; }
    public Action<PointerEventData, Transform> OnUpHandler { get; set; }
    public Action<PointerEventData, Transform> OnEnterHandler { get; set; }
    public Action<PointerEventData, Transform> OnExitHandler { get; set; }
    #endregion

    #region Interface Implementations
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
    #endregion
}
