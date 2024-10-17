using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public abstract class UI_Root : UI_Base
{   
    protected override void Init()
    {
        base.Init();

        Bind<UI_Panel>();
    }

    public UI_Panel GetPanel(string _name)
    {
        return Get<UI_Panel>(_name);
    }

    #region Show Or Hide Scene

    public async Task ShowScene(string name)
    {
        try
        {
            UI_Panel panel = GetPanel(name);
            await panel.ActiveWithMotion();
            Debug.Log($"{name} is successfully actived!");

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task HideScene(string name)
    {
        try
        {
            UI_Panel panel = GetPanel(name);
            await panel.DeactiveWithMotion();
            Debug.Log($"{name} is successfully deactived!");

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task ChangePanel(string prvName, string newName)
    {
        try
        {
            await HideScene(prvName);
            await ShowScene(newName);

            Debug.Log($"Successfully replaced from {prvName} panel to {newName} panel");
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    #endregion
}
