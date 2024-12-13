using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// UI 시스템의 최상위 관리자 클래스입니다.
/// 캔버스와 UI 패널들을 관리하고 전환을 처리합니다.
/// </summary>
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public abstract class UI_Root : UI_Base
{
    private Stack<UI_Popup> m_popupStack = new Stack<UI_Popup>();

    #region Initialization
    protected override void Init()
    {
        base.Init();
        Bind<UI_Panel>();
    }
    #endregion

    #region Panel Management
    /// <summary>
    /// 이름으로 UI 패널을 조회합니다.
    /// </summary>
    public UI_Panel GetPanel(string _name)
    {
        return Get<UI_Panel>(_name);
    }

    public UI_Popup GetPaopup(string _name)
    {
        return Get<UI_Popup>(_name);
    }

    /// <summary>
    /// 지정된 패널을 화면에 표시합니다.
    /// </summary>
    public async Task ShowPanel(UI_Panel panel, bool immediately = false)
    {
        try
        {
            if (immediately)
            {
                panel.gameObject.SetActive(true);
            }
            else
            {
                await panel.ActiveWithMotion();
            }
            Debug.Log($"{name} panel activated successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to show panel {name}: {ex.Message}");
        }
    }

    /// <summary>
    /// 지정된 패널을 화면에 표시합니다.
    /// </summary>
    public async Task ShowPanel(string name, bool immediately = false)
    {
        await ShowPanel(GetPanel(name), immediately);
    }

    /// <summary>
    /// 지정된 패널을 화면에서 숨깁니다.
    /// </summary>
    public async Task HidePanel(UI_Panel panel, bool immediately = false)
    {
        try
        {
            if (immediately)
            {
                panel.gameObject.SetActive(false);
            }
            else
            {
                await panel.DeactiveWithMotion();
            }
            Debug.Log($"{name} panel deactivated successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to hide panel {name}: {ex.Message}");
        }
    }

    /// <summary>
    /// 지정된 패널을 화면에서 숨깁니다.
    /// </summary>
    public async Task HidePanel(string name, bool immediately = false)
    {
        await HidePanel(GetPanel(name), immediately);
    }

    /// <summary>
    /// 현재 패널을 새로운 패널로 전환합니다.
    /// </summary>
    public async Task ChangePanel(string prvName, string newName, bool immediately = false)
    {
        try
        {
            await HidePanel(prvName, immediately);
            await ShowPanel(newName, immediately);
            Debug.Log($"Successfully switched from {prvName} to {newName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to change panel: {ex.Message}");
        }
    }

    /// <summary>
    /// 새 팝업을 열고 스택에 추가합니다.
    /// </summary>
    public async Task PushPopup(string name, bool immediately = false)
    {
        try
        {
            // 현재 팝업이 있다면 숨김
            if (m_popupStack.Contains(GetPaopup(name)))
            {
                await HidePanel(name, immediately);
            }

            // 새 패널 표시
            await ShowPanel(name, immediately);

            // 스택에 추가
            m_popupStack.Push(GetPaopup(name));

            Debug.Log($"Pushed panel: {name}. Stack size: {m_popupStack.Count}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to push panel {name}: {ex.Message}");
        }
    }

    /// <summary>
    /// 현재 팝업을 닫고 이전 패널로 돌아갑니다.
    /// </summary>
    public async Task PopPopup(bool immediately = false)
    {
        try
        {
            if (m_popupStack.Count <= 1)
            {
                Debug.LogWarning("Cannot pop the last panel.");
                return;
            }

            await HidePanel(m_popupStack.Pop(), immediately);

            // 현재 패널 스택에서 제거
            m_popupStack.Pop();

            // 이전 패널 활성화
            UI_Popup previousPanel = m_popupStack.Peek();
            await ShowPanel(previousPanel, immediately);

            Debug.Log($"Popped to panel: {previousPanel}. Stack size: {m_popupStack.Count}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to pop panel: {ex.Message}");
        }
    }

    /// <summary>
    /// 스택의 모든 팝업을 초기화합니다.
    /// </summary>
    public async Task ClearPopupStack(bool immediately = false)
    {
        try
        {
            while (m_popupStack.Count > 0)
            {
                UI_Popup panelToHide = m_popupStack.Pop();
                await HidePanel(panelToHide, immediately);
            }

            Debug.Log("Panel stack cleared.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to clear panel stack: {ex.Message}");
        }
    }
    #endregion
}
