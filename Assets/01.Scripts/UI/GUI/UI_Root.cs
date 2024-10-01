using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Canvas))]
public abstract class UI_Root : UI_Base
{
    protected Dictionary<string, UI_Scene> _scenes = new Dictionary<string, UI_Scene>();

    protected override void Init()
    {
        base.Init();

        Bind<Image>();
        Bind<TextMeshProUGUI>();
        Bind<Button>();
        Bind<Slider>();
    }

    protected override void Awake()
    {
        base.Awake();

        List<UI_Scene> uI_Scenes = Util.Util.FindChilds<UI_Scene>(gameObject);

        foreach (var item in uI_Scenes)
        {
            _scenes.Add(item.name, item);
        }
    }

    public UI_Scene GetScene(string _name)
    {
        return _scenes[_name];
    }

    #region Show Or Hide Scene

    public void ShowScene(string name, bool callback = false)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning($"{name} is empty");
            return;
        }

        if (!_scenes.ContainsKey(name))
        {
            Debug.LogWarning($"{name} does not exist");
            return;
        }

        if (callback)
        {
            _scenes[name].gameObject.SetActive(true);
            _scenes[name].ActiveWithMotion();
        }
        else
            _scenes[name].gameObject.SetActive(true);
    }

    public void ShowScene(UI_Scene scene)
    {
        if (!_scenes.ContainsValue(scene))
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

        if (!_scenes.ContainsKey(name))
        {
            Debug.LogWarning($"{name} does not exist");
            return;
        }

        if (callback)
            _scenes[name].DeactiveWithMotion();
        else
            _scenes[name].gameObject.SetActive(false);
    }

    public void HideScene(UI_Scene scene)
    {
        if (!_scenes.ContainsValue(scene))
        {
            Debug.LogWarning($"{scene.name} does not exist");
            return;
        }

        scene.DeactiveWithMotion();
    }

    #endregion
}
