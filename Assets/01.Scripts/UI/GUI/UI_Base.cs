using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public abstract class UI_Base : MonoBehaviour
{
    protected MultiKeyDictionary<Type, string, UnityEngine.Object> m_objects = new MultiKeyDictionary<Type, string, UnityEngine.Object>();

    protected virtual void Awake()
    {
    }
    protected virtual void OnEnable()
    {
        Init();
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
        if (gameObject.activeSelf == false)
            return;
    }

    protected virtual void Init()
    {

    }

    protected void Bind<T>() where T : UnityEngine.Object
    {
        List<T> list = Util.Util.FindChilds<T>(gameObject);

        foreach (T obj in list)
        {
            m_objects.Add(typeof(T), obj.name, obj);
        }
    }

    protected T Get<T>(string name) where T : UnityEngine.Object
    {
        return m_objects[typeof(T)][name] as T;
    }

    public static void BindEvent(GameObject obj, Action<PointerEventData, Transform> evt, Define.ClickType type = Define.ClickType.Click)
    {
        UI_EventHandler handle = Util.Util.GetOrAddComponent<UI_EventHandler>(obj);

        switch (type)
        {
            case Define.ClickType.Click:
                handle.OnClickHandler = evt;
                break;
            case Define.ClickType.Down:
                handle.OnDownHandler = evt;
                break;
            case Define.ClickType.Up:
                handle.OnUpHandler = evt;
                break;
            case Define.ClickType.Move:
                handle.OnMoveHandler = evt;
                break;
                //case Define.ClickType.OnDrag:
                //    handle.OnBeginDragHandler = evt;
                //    break;
                //case Define.ClickType.Dragging:
                //    handle.OnDragHandler = evt;
                //    break;
                //case Define.ClickType.EndDrag:
                //    handle.OnEndDragHandler = evt;
                //    break;
        }
    }
}
