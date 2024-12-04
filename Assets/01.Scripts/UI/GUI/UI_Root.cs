using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using System;

/// <summary>
/// UI 시스템의 최상위 관리자 클래스입니다.
/// 캔버스와 UI 패널들을 관리하고 전환을 처리합니다.
/// </summary>
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public abstract class UI_Root : UI_Base
{
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

    /// <summary>
    /// 지정된 패널을 화면에 표시합니다.
    /// </summary>
    public async Task ShowScene(string name)
    {
        try
        {
            UI_Panel panel = GetPanel(name);
            await panel.ActiveWithMotion();
            Debug.Log($"{name} panel activated successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to show panel {name}: {ex.Message}");
        }
    }

    /// <summary>
    /// 지정된 패널을 화면에서 숨깁니다.
    /// </summary>
    public async Task HideScene(string name)
    {
        try
        {
            UI_Panel panel = GetPanel(name);
            await panel.DeactiveWithMotion();
            Debug.Log($"{name} panel deactivated successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to hide panel {name}: {ex.Message}");
        }
    }

    /// <summary>
    /// 현재 패널을 새로운 패널로 전환합니다.
    /// </summary>
    public async Task ChangePanel(string prvName, string newName)
    {
        try
        {
            await HideScene(prvName);
            await ShowScene(newName);
            Debug.Log($"Successfully switched from {prvName} to {newName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to change panel: {ex.Message}");
        }
    }
    #endregion
}
