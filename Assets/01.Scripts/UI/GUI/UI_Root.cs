using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;
using UnityEngine.UI;
using TMPro;

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

    public void ShowScene(string name, bool callback = false)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning($"{name} is empty");
            return;
        }

        if (GetPanel(name) == null)
        {
            Debug.LogWarning($"{name} does not exist");
            return;
        }

        if (callback)
        {
            GetPanel(name).gameObject.SetActive(true);
            GetPanel(name).ActiveWithMotion();
        }
        else
            GetPanel(name).gameObject.SetActive(true);
    }

    public void ShowScene(UI_Panel scene)
    {
        if (GetPanel(name) == null)
        {
            Debug.LogWarning($"{scene.name} does not exist");
            return;
        }

        scene.ActiveWithMotion();
    }

    public void HideScene(string name, bool callback = false)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning($"{name} is empty");
            return;
        }

        if (GetPanel(name) == null)
        {
            Debug.LogWarning($"{name} does not exist");
            return;
        }

        if (callback)
            GetPanel(name).DeactiveWithMotion();
        else
            GetPanel(name).gameObject.SetActive(false);
    }

    public void HideScene(UI_Panel scene)
    {
        if (GetPanel(name) == null)
        {
            Debug.LogWarning($"{scene.name} does not exist");
            return;
        }

        scene.DeactiveWithMotion();
    }

    #endregion
}
