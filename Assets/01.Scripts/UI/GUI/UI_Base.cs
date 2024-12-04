using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Util;

/// <summary>
/// UI 시스템의 기본 클래스입니다.
/// 모든 UI 컴포넌트의 기본이 되는 추상 클래스로, 공통 기능을 제공합니다.
/// </summary>
public abstract class UI_Base : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// UI 오브젝트들을 Type과 이름으로 관리하는 다중 키 딕셔너리입니다.
    /// </summary>
    protected MultiKeyDictionary<Type, string, UnityEngine.Object> m_objects = new MultiKeyDictionary<Type, string, UnityEngine.Object>();
    #endregion

    #region Unity Lifecycle
    protected virtual void Awake() => Init();
    protected virtual void OnEnable() { }
    protected virtual void Start() { }
    protected virtual void Update() { }
    #endregion

    #region Initialization
    /// <summary>
    /// UI 컴포넌트를 초기화합니다. 하위 클래스에서 재정의하여 사용합니다.
    /// </summary>
    protected virtual void Init() { }

    /// <summary>
    /// 지정된 타입의 모든 자식 UI 컴포넌트를 찾아 바인딩합니다.
    /// </summary>
    /// <typeparam name="T">바인딩할 컴포넌트 타입</typeparam>
    protected void Bind<T>() where T : UnityEngine.Object
    {
        var list = Util.Util.FindChilds<T>(gameObject);
        foreach (T obj in list)
        {
            m_objects.Add(typeof(T), obj.name, obj);
        }
    }

    /// <summary>
    /// 바인딩된 컴포넌트를 이름으로 조회합니다.
    /// </summary>
    /// <typeparam name="T">조회할 컴포넌트 타입</typeparam>
    /// <param name="name">컴포넌트 이름</param>
    /// <returns>찾은 컴포넌트</returns>
    protected T Get<T>(string name) where T : UnityEngine.Object
    {
        return m_objects[typeof(T)][name] as T;
    }
    #endregion

    #region Event Handling
    /// <summary>
    /// UI 게임오브젝트에 이벤트 핸들러를 연결합니다.
    /// </summary>
    /// <param name="obj">이벤트를 연결할 게임오브젝트</param>
    /// <param name="evt">실행할 이벤트 액션</param>
    /// <param name="type">클릭 이벤트 타입</param>
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
            case Define.ClickType.Enter:
                handle.OnEnterHandler = evt;
                break;
            case Define.ClickType.Exit:
                handle.OnExitHandler = evt;
                break;
        }
    }
    #endregion
}
